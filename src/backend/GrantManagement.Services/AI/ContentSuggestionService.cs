using GrantManagement.Core.DTOs;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrantManagement.Services.AI;

public class ContentSuggestionService(
    IReportRepository reportRepo,
    IGrantRepository grantRepo,
    IAIRepository aiRepo,
    IOpenAIService openAI,
    ILogger<ContentSuggestionService> logger) : IContentSuggestionService
{
    // GPT-4-turbo pricing: $0.01 per 1K prompt tokens, $0.03 per 1K completion tokens
    private const decimal PromptCostPer1K = 0.01m;
    private const decimal CompletionCostPer1K = 0.03m;

    public async Task<SuggestionResponseDto> GenerateSuggestionAsync(SuggestionRequestDto request)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 1. Load the report and grant context
        var report = await reportRepo.GetByIdWithSectionsAsync(request.ReportId);
        if (report is null)
            return new SuggestionResponseDto { Success = false, ErrorMessage = "Report not found" };

        var grant = report.Grant;

        // 2. Get previous report content for continuity
        var previousContent = await aiRepo.GetPreviousReportContentAsync(grant.GrantId, request.SectionName);

        // 3. Find approved examples from similar grants
        var examples = await aiRepo.FindExamplesAsync(grant.ProgramTypeCode, request.SectionName, grant.GrantId);

        // 4. Build prompt
        // 4b. Collect already-filled sibling sections for cross-section coherence
        var siblingContext = report.Sections
            .Where(s => s.SectionName != request.SectionName &&
                        !string.IsNullOrWhiteSpace(s.ResponseText))
            .OrderBy(s => s.SectionOrder)
            .Take(3)
            .Select(s => (s.SectionName, s.SectionTitle,
                Snippet: s.ResponseText!.Length > 200 ? s.ResponseText![..200] + "…" : s.ResponseText!))
            .ToList();

        var systemPrompt = BuildSystemPrompt(request.Tone);
        var userPrompt = BuildUserPrompt(grant, report, request.SectionName, previousContent, examples,
            request.KeyPoints, request.RegenerationFeedback, siblingContext, request.WordCount);

        // 5. Call OpenAI
        var result = await openAI.CompleteAsync(systemPrompt, userPrompt, maxTokens: 400);

        sw.Stop();

        var cost = CalculateCost(result.PromptTokens, result.CompletionTokens);

        // 6. Log usage
        var logId = await aiRepo.LogUsageAsync(new AIUsageLog
        {
            UserId = request.UserId,
            GrantId = grant.GrantId,
            ReportId = request.ReportId,
            FeatureType = "ContentSuggestion",
            SectionName = request.SectionName,
            ModelName = "ollama/qwen2.5-coder",
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
            TotalTokens = result.PromptTokens + result.CompletionTokens,
            EstimatedCost = cost,
            ResponseTimeMs = (int)sw.ElapsedMilliseconds,
            Success = result.Success,
            ErrorMessage = result.Error
        });

        if (!result.Success)
        {
            logger.LogError("OpenAI suggestion failed: {Error}", result.Error);
            return new SuggestionResponseDto { Success = false, ErrorMessage = result.Error };
        }

        // 7. Score the suggestion quality in parallel with any other post-processing
        var qualityScore = await ScoreSuggestionAsync(result.Content!, request.SectionName, request.KeyPoints);

        return new SuggestionResponseDto
        {
            Success = true,
            SuggestedText = result.Content,
            TokensUsed = result.PromptTokens + result.CompletionTokens,
            EstimatedCost = cost,
            LogId = logId,
            QualityScore = qualityScore
        };
    }

    // Returns a quality score 1-5 for the suggestion based on relevance, specificity, and professional tone.
    private async Task<int?> ScoreSuggestionAsync(string suggestion, string sectionName, string? keyPoints)
    {
        var context = string.IsNullOrWhiteSpace(keyPoints)
            ? $"Section: {sectionName}"
            : $"Section: {sectionName}\nKey points that should be covered: {keyPoints}";

        var prompt = $"""
            {context}

            Generated narrative:
            {suggestion}

            Score this narrative 1–5 on these criteria:
            - Relevance: Does it address the section topic and key points?
            - Specificity: Does it include concrete details rather than vague statements?
            - Professionalism: Is the tone appropriate for a federal grant report?

            Reply with ONLY a single digit from 1 to 5. No explanation.
            """;

        var result = await openAI.CompleteAsync(
            "You are a grant report quality evaluator. Reply with a single digit 1-5.",
            prompt, maxTokens: 5);

        if (result.Success && int.TryParse(result.Content?.Trim(), out var score) && score is >= 1 and <= 5)
            return score;

        return null;
    }

    public async IAsyncEnumerable<string> StreamSuggestionAsync(SuggestionRequestDto request)
    {
        var report = await reportRepo.GetByIdWithSectionsAsync(request.ReportId);
        if (report is null) { yield return "[ERROR: Report not found]"; yield break; }

        var grant = report.Grant;
        var previousContent = await aiRepo.GetPreviousReportContentAsync(grant.GrantId, request.SectionName);
        var examples = await aiRepo.FindExamplesAsync(grant.ProgramTypeCode, request.SectionName, grant.GrantId);
        var siblingContext = report.Sections
            .Where(s => s.SectionName != request.SectionName && !string.IsNullOrWhiteSpace(s.ResponseText))
            .OrderBy(s => s.SectionOrder).Take(3)
            .Select(s => (s.SectionName, s.SectionTitle,
                Snippet: s.ResponseText!.Length > 200 ? s.ResponseText![..200] + "…" : s.ResponseText!))
            .ToList();

        var systemPrompt = BuildSystemPrompt(request.Tone);
        var userPrompt = BuildUserPrompt(grant, report, request.SectionName, previousContent, examples,
            request.KeyPoints, request.RegenerationFeedback, siblingContext, request.WordCount);

        var fullText = new System.Text.StringBuilder();
        await foreach (var token in openAI.StreamAsync(systemPrompt, userPrompt, maxTokens: 400))
        {
            fullText.Append(token);
            yield return token;
        }

        // Emit metadata as a final token so the client can read logId and qualityScore
        if (fullText.Length > 0)
        {
            var generatedText = fullText.ToString();
            var logId = await aiRepo.LogUsageAsync(new AIUsageLog
            {
                UserId = request.UserId,
                GrantId = report.Grant.GrantId,
                ReportId = request.ReportId,
                FeatureType = "ContentSuggestion",
                SectionName = request.SectionName,
                ModelName = "ollama/qwen2.5-coder",
                Success = true
            });
            var qualityScore = await ScoreSuggestionAsync(generatedText, request.SectionName, request.KeyPoints);
            yield return $"[META:{{\"logId\":\"{logId}\",\"qualityScore\":{(qualityScore.HasValue ? qualityScore.Value.ToString() : "null")}}}]";
        }
    }

    public async Task RecordFeedbackAsync(FeedbackRequestDto feedback)
    {
        await aiRepo.UpdateFeedbackAsync(feedback.LogId, feedback.UserAction, feedback.UserRating);

        // Promote accepted suggestions into the example pool for future generations
        if (feedback.UserAction is "Accepted" or "Edited" &&
            !string.IsNullOrWhiteSpace(feedback.AcceptedText))
        {
            _ = PromoteToApprovedContentAsync(feedback);
        }
    }

    private async Task PromoteToApprovedContentAsync(FeedbackRequestDto feedback)
    {
        try
        {
            var log = await aiRepo.GetUsageLogAsync(feedback.LogId);
            if (log?.SectionName is null || log.ReportId is null) return;

            var grant = await grantRepo.GetByIdAsync(log.GrantId);
            if (grant is null) return;

            await aiRepo.AddApprovedContentAsync(new AIApprovedContent
            {
                GrantId = log.GrantId,
                ReportId = log.ReportId,
                ProgramTypeCode = grant.ProgramTypeCode,
                SectionName = log.SectionName,
                Content = feedback.AcceptedText!,
                ApprovalDate = DateTime.UtcNow,
                ReviewerRating = feedback.UserRating ?? 4,
                GrantType = grant.GrantType
            });

            logger.LogInformation("Promoted accepted suggestion to AIApprovedContent for grant {GrantId} section {Section}",
                log.GrantId, log.SectionName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to promote suggestion to approved content for log {LogId}", feedback.LogId);
        }
    }

    private static string BuildSystemPrompt(string? tone = null)
    {
        var toneInstruction = (tone ?? "Professional").ToLower() switch
        {
            "concise"  => "Be very concise — use short, direct sentences. Avoid filler phrases.",
            "detailed" => "Be thorough and detailed. Provide context, explain outcomes, and include supporting evidence.",
            _          => "Be professional and clear. Use formal but accessible language."
        };
        return $"You are a grant writer for HRSA Health Center progress reports. Write in first person. {toneInstruction} Max 250 words.";
    }

    private static string BuildUserPrompt(
        Grant grant, Report report, string sectionName,
        string? previousContent, List<AIApprovedContent> examples,
        string? keyPoints, string? regenerationFeedback = null,
        List<(string SectionName, string SectionTitle, string Snippet)>? siblingContext = null,
        int? wordCount = null)
    {
        var sb = new System.Text.StringBuilder();
        var hasKeyPoints = !string.IsNullOrWhiteSpace(keyPoints);

        // When user provides key points, lead with them so the model anchors on them first
        if (hasKeyPoints)
        {
            sb.AppendLine($"""
                You are writing a {sectionName} narrative for a HRSA progress report.

                Grant: {grant.GrantNumber} ({grant.GrantType}) — {grant.ProgramName}
                Reporting Period: {report.ReportingYear} {report.ReportingQuarter}

                Key highlights for this period (treat these as facts — use them exactly as given, do not replace them with numbers from the previous report):
                {keyPoints!.Trim()}

                Previous report (use this to fill gaps, provide additional context, and match the writing style — but do not override the key highlights above with its numbers):
                """);

            if (!string.IsNullOrWhiteSpace(previousContent))
            {
                var prev = previousContent.Length > 400 ? previousContent[..400] + "…" : previousContent;
                sb.AppendLine(prev);
            }
            else
            {
                sb.AppendLine("No previous report available.");
            }
        }
        else
        {
            // No key points — standard prompt using all context
            sb.AppendLine($"""
                Generate a {sectionName} narrative for this progress report:

                Grant Information:
                - Grant Number: {grant.GrantNumber}
                - Grant Type: {grant.GrantType}
                - Program: {grant.ProgramName}
                - Focus Areas: {grant.FocusAreas ?? "Not specified"}
                - Reporting Period: {report.ReportingYear} {report.ReportingQuarter}
                """);

            if (!string.IsNullOrWhiteSpace(previousContent))
            {
                var prev = previousContent.Length > 300 ? previousContent[..300] + "…" : previousContent;
                sb.AppendLine($"\nPrevious report (for continuity): {prev}");
            }

            if (examples.Count > 0)
            {
                var ex = examples[0];
                var content = ex.Content?.Length > 300 ? ex.Content[..300] + "…" : ex.Content;
                sb.AppendLine($"\nApproved example (rating {ex.ReviewerRating}/5): {content}");
            }
        }

        var targetWords = wordCount is 100 or 150 or 200 or 250 ? wordCount.Value : 150;
        var wordRange = $"{targetWords - 20}-{targetWords + 20}";
        var instruction = hasKeyPoints
            ? $"Write a {wordRange} word narrative. Lead with the key highlights, then expand with relevant context from the previous report to fill out the full picture."
            : $"Write a {wordRange} word narrative. Be specific and outcome-focused.";

        // Other sections already written in this report — use for coherence, avoid contradictions
        if (siblingContext is { Count: > 0 })
        {
            sb.AppendLine("\nOther sections already written in this report (for consistency — do not contradict):");
            foreach (var (name, title, snippet) in siblingContext)
                sb.AppendLine($"  [{title.IfEmpty(name)}]: {snippet}");
        }

        // Refinement instruction for regeneration
        if (!string.IsNullOrWhiteSpace(regenerationFeedback))
            sb.AppendLine($"\nRefinement request: {regenerationFeedback.Trim()}");

        sb.AppendLine($"\n{instruction}\n\nNarrative:");

        return sb.ToString();
    }

    private static decimal CalculateCost(int promptTokens, int completionTokens) =>
        (promptTokens / 1000m * PromptCostPer1K) + (completionTokens / 1000m * CompletionCostPer1K);
}

file static class StringExtensions
{
    public static string IfEmpty(this string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
