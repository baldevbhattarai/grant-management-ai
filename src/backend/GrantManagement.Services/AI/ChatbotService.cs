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
    ILogger<ChatbotService> logger) : IChatbotService
{
    private const int MaxHistoryTurns = 5;

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

        // 5. Semantic vector search using the rewritten question — falls back to SQL LIKE if Qdrant unavailable
        List<ChatSourceDto> sources;
        string contextBlock;
        float? confidenceScore;
        (sources, contextBlock, confidenceScore) = await BuildContextAsync(standaloneQuestion, request.GrantId);

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

        // 8. Persist this turn to conversation history
        var userId = request.UserId ?? Guid.Empty;
        await chatRepo.SaveTurnAsync(sessionId, userId, request.GrantId, request.Question, result.Content!);

        return new ChatResponseDto
        {
            Success = true,
            Answer = result.Content,
            ConversationId = sessionId,
            Sources = sources,
            ConfidenceScore = confidenceScore
        };
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

    // Returns (sourceDtos, context text block, maxConfidenceScore) — tries vector search first, falls back to SQL LIKE
    private async Task<(List<ChatSourceDto>, string, float?)> BuildContextAsync(string question, Guid grantId)
    {
        try
        {
            var queryVector = await embeddingService.EmbedAsync(question);
            var vectorResults = await vectorService.SearchAsync(queryVector, grantId, topN: 3, minScore: 0.45f);

            if (vectorResults.Count > 0)
            {
                logger.LogDebug("Vector search returned {Count} results for grant {GrantId}", vectorResults.Count, grantId);

                var maxScore = vectorResults.Max(r => r.Score);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Report context (semantic search):");
                foreach (var r in vectorResults)
                {
                    var snippet = r.ResponseText.Length > 300 ? r.ResponseText[..300] + "…" : r.ResponseText;
                    sb.AppendLine($"[{r.ReportingYear} {r.ReportingQuarter} - {r.SectionName}]: {snippet}");
                }

                var dtos = vectorResults.Select(r => new ChatSourceDto
                {
                    ReportPeriod = $"{r.ReportingYear} {r.ReportingQuarter}",
                    SectionName = r.SectionName,
                    Snippet = r.ResponseText.Length > 200 ? r.ResponseText[..200] + "..." : r.ResponseText
                }).ToList();

                return (dtos, sb.ToString(), maxScore);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Vector search unavailable — falling back to keyword search");
        }

        // SQL LIKE keyword fallback — no confidence score available
        var (fallbackDtos, fallbackContext) = await KeywordFallbackAsync(question, grantId);
        return (fallbackDtos, fallbackContext, null);
    }

    private async Task<(List<ChatSourceDto>, string)> KeywordFallbackAsync(string question, Guid grantId)  // returns without confidence score
    {
        var keywords = ExtractKeywords(question);
        var sections = new List<ReportSection>();
        foreach (var kw in keywords)
        {
            var found = await aiRepo.SearchSectionsAsync(grantId, kw, topN: 3);
            sections.AddRange(found);
        }

        var unique = sections.DistinctBy(s => s.SectionId).Take(2).ToList();

        var sb = new System.Text.StringBuilder();
        if (unique.Count > 0)
        {
            sb.AppendLine("Report context (keyword search):");
            foreach (var s in unique)
            {
                var snippet = s.ResponseText?.Length > 300 ? s.ResponseText[..300] + "…" : s.ResponseText ?? string.Empty;
                sb.AppendLine($"[{s.Report.ReportingYear} {s.Report.ReportingQuarter} - {s.SectionName}]: {snippet}");
            }
        }

        var dtos = unique.Select(s => new ChatSourceDto
        {
            ReportPeriod = $"{s.Report.ReportingYear} {s.Report.ReportingQuarter}",
            SectionName = s.SectionName,
            Snippet = s.ResponseText?.Length > 200 ? s.ResponseText[..200] + "..." : s.ResponseText ?? string.Empty
        }).ToList();

        return (dtos, sb.ToString());
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

        // Conversation history (prior turns in this session)
        if (history.Count > 0)
        {
            sb.AppendLine("Conversation so far:");
            foreach (var msg in history)
            {
                var label = msg.Role == "user" ? "User" : "Assistant";
                sb.AppendLine($"{label}: {msg.Content}");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"Current question: {question}");
        return sb.ToString();
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
