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
    ILogger<ChatbotService> logger) : IChatbotService
{
    public async Task<ChatResponseDto> AskAsync(ChatRequestDto request)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 1. Load grant context
        var grant = await grantRepo.GetByIdAsync(request.GrantId);
        if (grant is null)
            return new ChatResponseDto { Success = false, ErrorMessage = "Grant not found" };

        // 2. Semantic vector search — falls back to SQL LIKE if Qdrant unavailable
        List<ChatSourceDto> sources;
        string contextBlock;
        (sources, contextBlock) = await BuildContextAsync(request.Question, request.GrantId);

        // 3. Build prompt with context
        var systemPrompt = BuildSystemPrompt(grant);
        var userPrompt = BuildUserPrompt(request.Question, contextBlock);

        // 4. Call LLM
        var result = await openAI.CompleteAsync(systemPrompt, userPrompt, maxTokens: 300);

        sw.Stop();

        // 5. Log usage
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

        return new ChatResponseDto
        {
            Success = true,
            Answer = result.Content,
            ConversationId = request.ConversationId ?? Guid.NewGuid(),
            Sources = sources
        };
    }

    // Returns (sourceDtos, context text block) — tries vector search first, falls back to SQL LIKE
    private async Task<(List<ChatSourceDto>, string)> BuildContextAsync(string question, Guid grantId)
    {
        try
        {
            var queryVector = await embeddingService.EmbedAsync(question);
            var vectorResults = await vectorService.SearchAsync(queryVector, grantId, topN: 3, minScore: 0.45f);

            if (vectorResults.Count > 0)
            {
                logger.LogDebug("Vector search returned {Count} results for grant {GrantId}", vectorResults.Count, grantId);

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

                return (dtos, sb.ToString());
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Vector search unavailable — falling back to keyword search");
        }

        // SQL LIKE keyword fallback
        return await KeywordFallbackAsync(question, grantId);
    }

    private async Task<(List<ChatSourceDto>, string)> KeywordFallbackAsync(string question, Guid grantId)
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

    private static string BuildSystemPrompt(Core.Entities.Grant grant) =>
        $"You are an assistant for HRSA grant {grant.GrantNumber} ({grant.GrantType}). Answer based only on the provided report content. Be brief and factual.";

    private static string BuildUserPrompt(string question, string contextBlock)
    {
        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrWhiteSpace(contextBlock))
        {
            sb.AppendLine(contextBlock);
            sb.AppendLine();
        }
        sb.AppendLine($"Question: {question}");
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
