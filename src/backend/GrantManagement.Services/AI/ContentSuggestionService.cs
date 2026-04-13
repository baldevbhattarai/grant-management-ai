using GrantManagement.Core.DTOs;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrantManagement.Services.AI;

public class ContentSuggestionService(
    IReportRepository reportRepo,
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
        var systemPrompt = BuildSystemPrompt();
        var userPrompt = BuildUserPrompt(grant, report, request.SectionName, previousContent, examples);

        // 5. Call OpenAI
        var result = await openAI.CompleteAsync(systemPrompt, userPrompt, maxTokens: 400);

        sw.Stop();

        var cost = CalculateCost(result.PromptTokens, result.CompletionTokens);

        // 6. Log usage
        await aiRepo.LogUsageAsync(new AIUsageLog
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

        return new SuggestionResponseDto
        {
            Success = true,
            SuggestedText = result.Content,
            TokensUsed = result.PromptTokens + result.CompletionTokens,
            EstimatedCost = cost
        };
    }

    public async Task RecordFeedbackAsync(FeedbackRequestDto feedback)
    {
        await aiRepo.UpdateFeedbackAsync(feedback.LogId, feedback.UserAction, feedback.UserRating);
    }

    private static string BuildSystemPrompt() =>
        "You are a grant writer for HRSA Health Center progress reports. Write in first person. Be concise and professional. Max 250 words.";

    private static string BuildUserPrompt(
        Grant grant, Report report, string sectionName,
        string? previousContent, List<AIApprovedContent> examples)
    {
        var sb = new System.Text.StringBuilder();

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

        sb.AppendLine("\nWrite a 150-200 word narrative for the section above. Be specific and outcome-focused.\n\nNarrative:");

        return sb.ToString();
    }

    private static decimal CalculateCost(int promptTokens, int completionTokens) =>
        (promptTokens / 1000m * PromptCostPer1K) + (completionTokens / 1000m * CompletionCostPer1K);
}
