using GrantManagement.Core.DTOs;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrantManagement.Services.AI;

public class ChatbotService(
    IGrantRepository grantRepo,
    IAIRepository aiRepo,
    IOpenAIService openAI,
    ILogger<ChatbotService> logger) : IChatbotService
{
    public async Task<ChatResponseDto> AskAsync(ChatRequestDto request)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 1. Load grant context
        var grant = await grantRepo.GetByIdAsync(request.GrantId);
        if (grant is null)
            return new ChatResponseDto { Success = false, ErrorMessage = "Grant not found" };

        // 2. Search relevant report sections (keyword search)
        var keywords = ExtractKeywords(request.Question);
        var sources = new List<ReportSection>();
        foreach (var kw in keywords)
        {
            var found = await aiRepo.SearchSectionsAsync(request.GrantId, kw, topN: 3);
            sources.AddRange(found);
        }

        // Deduplicate by section ID — limit to 2 to keep prompt small
        var uniqueSections = sources.DistinctBy(s => s.SectionId).Take(2).ToList();

        // 3. Build prompt with context
        var systemPrompt = BuildSystemPrompt(grant);
        var userPrompt = BuildUserPrompt(request.Question, uniqueSections);

        // 4. Call OpenAI
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
            logger.LogError("Chatbot OpenAI call failed: {Error}", result.Error);
            return new ChatResponseDto { Success = false, ErrorMessage = result.Error };
        }

        var conversationId = request.ConversationId ?? Guid.NewGuid();

        return new ChatResponseDto
        {
            Success = true,
            Answer = result.Content,
            ConversationId = conversationId,
            Sources = uniqueSections.Select(s => new ChatSourceDto
            {
                ReportPeriod = $"{s.Report.ReportingYear} {s.Report.ReportingQuarter}",
                SectionName = s.SectionName,
                Snippet = s.ResponseText?.Length > 200
                    ? s.ResponseText[..200] + "..."
                    : s.ResponseText ?? string.Empty
            }).ToList()
        };
    }

    private static string BuildSystemPrompt(Core.Entities.Grant grant) =>
        $"You are an assistant for HRSA grant {grant.GrantNumber} ({grant.GrantType}). Answer based only on the provided report content. Be brief and factual.";

    private static string BuildUserPrompt(string question, List<ReportSection> sections)
    {
        var sb = new System.Text.StringBuilder();

        if (sections.Count > 0)
        {
            sb.AppendLine("Report context:");
            foreach (var s in sections)
            {
                var snippet = s.ResponseText?.Length > 300 ? s.ResponseText[..300] + "…" : s.ResponseText ?? string.Empty;
                sb.AppendLine($"[{s.Report.ReportingYear} {s.Report.ReportingQuarter} - {s.SectionName}]: {snippet}");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"Question: {question}");
        return sb.ToString();
    }

    private static List<string> ExtractKeywords(string question)
    {
        // Simple keyword extraction — strips common stop words
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
