using GrantManagement.Core.DTOs;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrantManagement.Services.AI;

public class ChatbotService(
    IGrantRepository grantRepo,
    IAIRepository aiRepo,
    IOpenAIService openAI,
    IEmbeddingService embeddingService,
    IVectorSearchService vectorService,
    IChatRepository chatRepo,
    IRerankService rerankService,
    ILogger<ChatbotService> logger) : IChatbotService
{
    private const int MaxHistoryTurns = 5;
    private const int SummarizationThreshold = 8; // summarize after 8 turns to keep context tight

    public async Task<ChatResponseDto> AskAsync(ChatRequestDto request)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 1. Resolve session — reuse existing or start a new one
        var sessionId = request.ConversationId ?? Guid.NewGuid();

        // 2. Load grant context
        var grant = await grantRepo.GetByIdAsync(request.GrantId);
        if (grant is null)
            return new ChatResponseDto { Success = false, ErrorMessage = "Grant not found" };

        // 3. Load conversation history first so query rewriting has context
        var history = await chatRepo.GetHistoryAsync(sessionId, MaxHistoryTurns);

        // 4. Rewrite vague/contextual questions into standalone queries before embedding
        var standaloneQuestion = await RewriteQuestionAsync(request.Question, history);
        logger.LogDebug("Query rewrite: '{Original}' → '{Rewritten}'", request.Question, standaloneQuestion);

        // 5. Detect structured data intent — if the user asks for numbers/metrics, query DB directly
        List<ChatSourceDto> sources;
        string contextBlock;
        float? confidenceScore;

        var structuredResult = await TryStructuredQueryAsync(standaloneQuestion, request.GrantId);
        if (structuredResult is not null)
        {
            (sources, contextBlock) = structuredResult.Value;
            confidenceScore = 1.0f; // DB data is exact — full confidence
            logger.LogDebug("Structured data intent detected for question: {Question}", standaloneQuestion);
        }
        else
        {
            (sources, contextBlock, confidenceScore) = await BuildContextAsync(standaloneQuestion, request.GrantId);
        }

        // 6. Build prompt with report context + history + current question
        var systemPrompt = BuildSystemPrompt(grant, confidenceScore);
        var userPrompt = BuildUserPrompt(request.Question, contextBlock, history);

        // 7. Call LLM
        var result = await openAI.CompleteAsync(systemPrompt, userPrompt, maxTokens: 300);

        sw.Stop();

        // 7. Log usage
        await aiRepo.LogUsageAsync(new AIUsageLog
        {
            UserId = request.UserId,
            GrantId = request.GrantId,
            FeatureType = "QA_Chatbot",
            Question = request.Question,
            ModelName = "ollama/qwen2.5-coder",
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
            TotalTokens = result.PromptTokens + result.CompletionTokens,
            ResponseTimeMs = (int)sw.ElapsedMilliseconds,
            Success = result.Success,
            ErrorMessage = result.Error
        });

        if (!result.Success)
        {
            logger.LogError("Chatbot LLM call failed: {Error}", result.Error);
            return new ChatResponseDto { Success = false, ErrorMessage = result.Error };
        }

        // 8. Generate follow-up questions and persist turn in parallel
        var followUpTask = GenerateFollowUpQuestionsAsync(request.Question, result.Content!, grant);
        var userId = request.UserId ?? Guid.Empty;
        var saveTask = chatRepo.SaveTurnAsync(sessionId, userId, request.GrantId, request.Question, result.Content!);

        await Task.WhenAll(followUpTask, saveTask);

        // 9. Summarize old turns in the background if session is getting long
        _ = TrySummarizeSessionAsync(sessionId, userId, request.GrantId, grant);

        return new ChatResponseDto
        {
            Success = true,
            Answer = result.Content,
            ConversationId = sessionId,
            Sources = sources,
            ConfidenceScore = confidenceScore,
            FollowUpQuestions = followUpTask.Result
        };
    }

    public async IAsyncEnumerable<string> AskStreamAsync(ChatRequestDto request)
    {
        var sessionId = request.ConversationId ?? Guid.NewGuid();

        var grant = await grantRepo.GetByIdAsync(request.GrantId);
        if (grant is null) { yield return "[ERROR: Grant not found]"; yield break; }

        var history = await chatRepo.GetHistoryAsync(sessionId, MaxHistoryTurns);
        var standaloneQuestion = await RewriteQuestionAsync(request.Question, history);

        string contextBlock;
        float? confidenceScore;
        var structuredResult = await TryStructuredQueryAsync(standaloneQuestion, request.GrantId);
        if (structuredResult is not null)
        {
            (_, contextBlock) = structuredResult.Value;
            confidenceScore = 1.0f;
        }
        else
        {
            (_, contextBlock, confidenceScore) = await BuildContextAsync(standaloneQuestion, request.GrantId);
        }

        var systemPrompt = BuildSystemPrompt(grant, confidenceScore);
        var userPrompt = BuildUserPrompt(request.Question, contextBlock, history);

        var fullAnswer = new System.Text.StringBuilder();

        // Yield the conversation ID as the very first SSE token so the client can track the session
        yield return $"[SESSION:{sessionId}]";

        await foreach (var token in openAI.StreamAsync(systemPrompt, userPrompt, maxTokens: 300))
        {
            fullAnswer.Append(token);
            yield return token;
        }

        // Persist turn after streaming completes (fire-and-forget errors are swallowed by the background task)
        var userId = request.UserId ?? Guid.Empty;
        var answerText = fullAnswer.ToString();
        if (!string.IsNullOrWhiteSpace(answerText))
        {
            _ = Task.Run(async () =>
            {
                await chatRepo.SaveTurnAsync(sessionId, userId, request.GrantId, request.Question, answerText);
                await TrySummarizeSessionAsync(sessionId, userId, request.GrantId, grant);
            });
        }
    }

    // Checks if the session has exceeded SummarizationThreshold turns and collapses old turns into a summary.
    private async Task TrySummarizeSessionAsync(Guid sessionId, Guid userId, Guid grantId, Core.Entities.Grant grant)
    {
        try
        {
            var turnCount = await chatRepo.GetTurnCountAsync(sessionId);
            if (turnCount < SummarizationThreshold) return;

            var allHistory = await chatRepo.GetHistoryAsync(sessionId, maxTurns: SummarizationThreshold);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Conversation history:");
            foreach (var msg in allHistory.Where(m => m.Role != "summary"))
            {
                var label = msg.Role == "user" ? "User" : "Assistant";
                sb.AppendLine($"{label}: {msg.Content}");
            }
            sb.AppendLine();
            sb.AppendLine($"Summarize the above conversation about HRSA grant {grant.GrantNumber} in 3-5 sentences, preserving key facts and decisions. Be concise.");

            var result = await openAI.CompleteAsync(
                "You are a conversation summarizer. Create a concise factual summary.",
                sb.ToString(),
                maxTokens: 200);

            if (result.Success && !string.IsNullOrWhiteSpace(result.Content))
            {
                await chatRepo.ReplaceTurnsWithSummaryAsync(sessionId, userId, grantId, result.Content.Trim());
                logger.LogInformation("Session {SessionId} summarized after {TurnCount} turns", sessionId, turnCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to summarize session {SessionId} — continuing without summarization", sessionId);
        }
    }

    // Generates 2-3 follow-up questions the user might want to ask next, based on the Q&A exchange.
    private async Task<List<string>> GenerateFollowUpQuestionsAsync(
        string question, string answer, Core.Entities.Grant grant)
    {
        var prompt = $"""
            Grant: {grant.GrantNumber} ({grant.GrantType})
            User asked: {question}
            Assistant answered: {answer}

            Generate exactly 3 short follow-up questions the user might want to ask next about this grant.
            Return ONLY a numbered list like:
            1. Question one?
            2. Question two?
            3. Question three?
            """;

        var result = await openAI.CompleteAsync(
            "You are a helpful assistant generating follow-up questions. Be concise.",
            prompt,
            maxTokens: 120);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Content))
            return [];

        return result.Content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => System.Text.RegularExpressions.Regex.Replace(l.Trim(), @"^\d+\.\s*", ""))
            .Where(l => l.Length > 5 && l.Contains('?'))
            .Take(3)
            .ToList();
    }

    // Rewrites vague/contextual questions into self-contained queries using conversation history.
    // Falls back to the original question if the LLM call fails or history is empty.
    private async Task<string> RewriteQuestionAsync(string question, List<Core.Entities.ChatConversation> history)
    {
        if (history.Count == 0) return question;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Conversation history:");
        foreach (var msg in history)
        {
            var label = msg.Role == "user" ? "User" : "Assistant";
            sb.AppendLine($"{label}: {msg.Content}");
        }
        sb.AppendLine();
        sb.AppendLine($"Current question: {question}");
        sb.AppendLine();
        sb.AppendLine("Rewrite the current question as a complete, standalone question that can be understood without the conversation history. Return ONLY the rewritten question, nothing else.");

        var result = await openAI.CompleteAsync(
            "You are a query rewriting assistant. Rewrite questions to be self-contained.",
            sb.ToString(),
            maxTokens: 80);

        return result.Success && !string.IsNullOrWhiteSpace(result.Content)
            ? result.Content.Trim('"', ' ', '\n')
            : question;
    }

    // Hybrid RAG: runs vector search and keyword search in parallel, fuses results with
    // Reciprocal Rank Fusion (RRF). Falls back to keyword-only if vector search fails.
    private async Task<(List<ChatSourceDto>, string, float?)> BuildContextAsync(string question, Guid grantId)
    {
        // Embed and keyword-extract in parallel
        var embedTask = embeddingService.EmbedAsync(question);
        var keywords = ExtractKeywords(question);
        var queryVector = await embedTask;

        // Launch both searches concurrently
        var vectorTask = Task.Run(async () =>
        {
            try { return await vectorService.SearchAsync(queryVector, grantId, topN: 5, minScore: 0.4f); }
            catch (Exception ex) { logger.LogWarning(ex, "Vector search failed — using keyword results only"); return new List<VectorSearchResult>(); }
        });

        var keywordTask = Task.Run(async () =>
        {
            var sections = new List<ReportSection>();
            foreach (var kw in keywords)
                sections.AddRange(await aiRepo.SearchSectionsAsync(grantId, kw, topN: 3));
            return sections.DistinctBy(s => s.SectionId).Take(5).ToList();
        });

        await Task.WhenAll(vectorTask, keywordTask);

        var vectorResults = vectorTask.Result;
        var keywordSections = keywordTask.Result;

        // Convert keyword sections to VectorSearchResult for unified RRF input
        var keywordResults = keywordSections.Select((s, i) => new VectorSearchResult(
            SectionId: s.SectionId,
            Score: 1.0f / (60 + i + 1),   // synthetic score for RRF ordering
            ResponseText: s.ResponseText ?? string.Empty,
            SectionName: s.SectionName,
            ReportingYear: s.Report?.ReportingYear ?? 0,
            ReportingQuarter: s.Report?.ReportingQuarter ?? string.Empty,
            ReportId: s.ReportId)).ToList();

        var hasVector = vectorResults.Count > 0;
        var hasKeyword = keywordResults.Count > 0;

        if (!hasVector && !hasKeyword)
            return ([], "No relevant report content found.", null);

        // Fuse with RRF when both sources have results; otherwise use whichever is available
        var merged = (hasVector && hasKeyword)
            ? FuseWithRRF(vectorResults, keywordResults, topN: 4)
            : hasVector ? vectorResults.Take(4).ToList()
                        : keywordResults.Take(4).ToList();

        var label = (hasVector && hasKeyword) ? "hybrid (semantic + keyword)" : hasVector ? "semantic" : "keyword";
        var maxScore = hasVector ? vectorResults.Max(r => r.Score) : (float?)null;

        // Re-rank fused results with cross-encoder if Cohere API key is configured
        if (rerankService.IsEnabled && merged.Count > 1)
        {
            var docs = merged.Select(r => r.ResponseText).ToList();
            var rerankedIndices = await rerankService.RerankAsync(question, docs, topN: merged.Count);
            merged = rerankedIndices.Select(i => merged[i]).ToList();
            label += " + reranked";
        }

        logger.LogDebug("Hybrid RAG [{Label}]: {VectorCount} vector + {KeywordCount} keyword → {MergedCount} merged",
            label, vectorResults.Count, keywordResults.Count, merged.Count);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Report context ({label} search):");
        foreach (var r in merged)
        {
            var snippet = r.ResponseText.Length > 300 ? r.ResponseText[..300] + "…" : r.ResponseText;
            sb.AppendLine($"[{r.ReportingYear} {r.ReportingQuarter} - {r.SectionName}]: {snippet}");
        }

        var dtos = merged.Select(r => new ChatSourceDto
        {
            ReportPeriod = $"{r.ReportingYear} {r.ReportingQuarter}",
            SectionName = r.SectionName,
            Snippet = r.ResponseText.Length > 200 ? r.ResponseText[..200] + "..." : r.ResponseText,
            ReportId = r.ReportId
        }).ToList();

        return (dtos, sb.ToString(), maxScore);
    }

    // Reciprocal Rank Fusion: score(d) = Σ 1/(k + rank). k=60 is the standard constant.
    private static List<VectorSearchResult> FuseWithRRF(
        List<VectorSearchResult> vectorResults,
        List<VectorSearchResult> keywordResults,
        int topN = 4, int k = 60)
    {
        var scores = new Dictionary<Guid, double>();
        var resultMap = new Dictionary<Guid, VectorSearchResult>();

        for (var i = 0; i < vectorResults.Count; i++)
        {
            var r = vectorResults[i];
            scores[r.SectionId] = scores.GetValueOrDefault(r.SectionId) + 1.0 / (k + i + 1);
            resultMap[r.SectionId] = r;
        }

        for (var i = 0; i < keywordResults.Count; i++)
        {
            var r = keywordResults[i];
            scores[r.SectionId] = scores.GetValueOrDefault(r.SectionId) + 1.0 / (k + i + 1);
            resultMap.TryAdd(r.SectionId, r);
        }

        return scores
            .OrderByDescending(kvp => kvp.Value)
            .Take(topN)
            .Select(kvp => resultMap[kvp.Key] with { Score = (float)kvp.Value })
            .ToList();
    }

    private static string BuildSystemPrompt(Core.Entities.Grant grant, float? confidenceScore = null)
    {
        var base_ = $"You are an assistant for HRSA grant {grant.GrantNumber} ({grant.GrantType}). Answer based only on the provided report content and conversation history. Be brief and factual.";

        // Low confidence: instruct the LLM to admit uncertainty rather than hallucinate
        if (confidenceScore is null || confidenceScore < 0.55f)
            base_ += " If the provided context does not contain enough information to answer confidently, say \"I don't have enough information in the grant reports to answer that.\" Do not guess or invent details.";

        return base_;
    }

    private static string BuildUserPrompt(string question, string contextBlock, List<Core.Entities.ChatConversation> history)
    {
        var sb = new System.Text.StringBuilder();

        // Report context from vector/keyword search
        if (!string.IsNullOrWhiteSpace(contextBlock))
        {
            sb.AppendLine(contextBlock);
            sb.AppendLine();
        }

        // Conversation history (prior turns in this session, may include a summary row)
        if (history.Count > 0)
        {
            foreach (var msg in history)
            {
                if (msg.Role == "summary")
                {
                    sb.AppendLine("Earlier conversation summary:");
                    sb.AppendLine(msg.Content);
                }
                else
                {
                    if (!sb.ToString().Contains("Conversation so far:"))
                        sb.AppendLine("Conversation so far:");
                    var label = msg.Role == "user" ? "User" : "Assistant";
                    sb.AppendLine($"{label}: {msg.Content}");
                }
            }
            sb.AppendLine();
        }

        sb.AppendLine($"Current question: {question}");
        return sb.ToString();
    }

    // Maps metric keywords to the section name fragment to search in the DB
    private static readonly Dictionary<string[], string> StructuredIntentMap = new(new StringArrayComparer())
    {
        { ["patient", "patients", "beneficiar", "served", "encounter"] , "patient" },
        { ["visit", "visits", "appointment"] , "visit" },
        { ["staff", "fte", "workforce", "employee"] , "staff" },
        { ["cost", "expenditure", "spend", "budget", "expense"] , "cost" },
        { ["revenue", "income", "billing", "charge"] , "revenue" },
    };

    private static string? DetectStructuredIntent(string question)
    {
        var lower = question.ToLower();
        // Only trigger for questions that ask for quantities/counts
        var isQuantityQuestion = new[] { "how many", "how much", "count", "total", "number of", "quantity", "amount" }
            .Any(lower.Contains);
        if (!isQuantityQuestion) return null;

        foreach (var (keywords, sectionHint) in StructuredIntentMap)
            if (keywords.Any(lower.Contains))
                return sectionHint;

        return null;
    }

    // Returns structured DB context if the question matches a data intent, otherwise null.
    private async Task<(List<ChatSourceDto>, string)?> TryStructuredQueryAsync(string question, Guid grantId)
    {
        var sectionHint = DetectStructuredIntent(question);
        if (sectionHint is null) return null;

        var sections = await aiRepo.GetStructuredDataAsync(grantId, sectionHint, topN: 4);
        if (sections.Count == 0) return null;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Structured report data (exact values from database):");
        foreach (var s in sections)
        {
            var value = s.ResponseNumber.HasValue
                ? s.ResponseNumber.Value.ToString("N0")
                : (s.ResponseSingle ?? s.ResponseText ?? "N/A");
            sb.AppendLine($"[{s.Report.ReportingYear} {s.Report.ReportingQuarter} - {s.SectionName}]: {value}");
        }

        var dtos = sections.Select(s => new ChatSourceDto
        {
            ReportPeriod = $"{s.Report.ReportingYear} {s.Report.ReportingQuarter}",
            SectionName = s.SectionName,
            Snippet = s.ResponseNumber.HasValue ? s.ResponseNumber.Value.ToString("N0") : (s.ResponseText?.Length > 100 ? s.ResponseText[..100] + "..." : s.ResponseText ?? string.Empty),
            ReportId = s.ReportId
        }).ToList();

        return (dtos, sb.ToString());
    }

    private static List<string> ExtractKeywords(string question)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "what", "when", "where", "who", "how", "did", "does", "is", "are",
            "was", "were", "the", "a", "an", "i", "my", "we", "our", "about",
            "in", "on", "at", "to", "for", "of", "and", "or", "with"
        };

        return question
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim('.', ',', '?', '!'))
            .Where(w => w.Length > 3 && !stopWords.Contains(w))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }
}

// Allows string[] as dictionary key by comparing element equality
file sealed class StringArrayComparer : IEqualityComparer<string[]>
{
    public bool Equals(string[]? x, string[]? y) => x is not null && y is not null && x.SequenceEqual(y);
    public int GetHashCode(string[] obj) => obj.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode()));
}
