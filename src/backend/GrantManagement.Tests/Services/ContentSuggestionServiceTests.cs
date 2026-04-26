using GrantManagement.Core.DTOs;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using GrantManagement.Services.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GrantManagement.Tests.Services;

public class ContentSuggestionServiceTests
{
    private readonly Mock<IReportRepository> _reportRepo = new();
    private readonly Mock<IGrantRepository> _grantRepo = new();
    private readonly Mock<IAIRepository> _aiRepo = new();
    private readonly Mock<IOpenAIService> _openAI = new();

    private ContentSuggestionService CreateSut() =>
        new(_reportRepo.Object, _grantRepo.Object, _aiRepo.Object, _openAI.Object,
            NullLogger<ContentSuggestionService>.Instance);

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateSuggestion_WhenOpenAISucceeds_ReturnsSuggestion()
    {
        // Arrange
        var grantId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var grant = new Grant
        {
            GrantId = grantId,
            GrantNumber = "GX-2024-00001",
            GrantType = "C16",
            ProgramName = "Community Health",
            ProgramTypeCode = 60
        };
        var report = new Report
        {
            ReportId = reportId,
            GrantId = grantId,
            ReportingYear = 2025,
            ReportingQuarter = "Q1",
            Grant = grant,
            Sections = []
        };

        _reportRepo.Setup(r => r.GetByIdWithSectionsAsync(reportId)).ReturnsAsync(report);
        _aiRepo.Setup(r => r.GetPreviousReportContentAsync(grantId, "PerformanceNarrative"))
               .ReturnsAsync("Previous narrative text");
        _aiRepo.Setup(r => r.FindExamplesAsync(60, "PerformanceNarrative", grantId, 3))
               .ReturnsAsync([]);
        _aiRepo.Setup(r => r.LogUsageAsync(It.IsAny<AIUsageLog>()))
               .ReturnsAsync(Guid.NewGuid());
        _openAI.Setup(o => o.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync(new OpenAIResult(true, "Generated narrative text.", 500, 300, null));

        // Act
        var result = await CreateSut().GenerateSuggestionAsync(new SuggestionRequestDto
        {
            ReportId = reportId,
            SectionName = "PerformanceNarrative",
            UserId = Guid.NewGuid()
        });

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Generated narrative text.", result.SuggestedText);
        Assert.Equal(800, result.TokensUsed);
        Assert.True(result.EstimatedCost > 0);
    }

    [Fact]
    public async Task GenerateSuggestion_WhenOpenAIFails_ReturnsFailure()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var grant = new Grant { GrantId = Guid.NewGuid(), ProgramTypeCode = 60 };
        var report = new Report { ReportId = reportId, Grant = grant, Sections = [] };

        _reportRepo.Setup(r => r.GetByIdWithSectionsAsync(reportId)).ReturnsAsync(report);
        _aiRepo.Setup(r => r.GetPreviousReportContentAsync(It.IsAny<Guid>(), It.IsAny<string>()))
               .ReturnsAsync((string?)null);
        _aiRepo.Setup(r => r.FindExamplesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
               .ReturnsAsync([]);
        _aiRepo.Setup(r => r.LogUsageAsync(It.IsAny<AIUsageLog>())).ReturnsAsync(Guid.NewGuid());
        _openAI.Setup(o => o.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync(new OpenAIResult(false, null, 0, 0, "API key not configured"));

        // Act
        var result = await CreateSut().GenerateSuggestionAsync(new SuggestionRequestDto
        {
            ReportId = reportId,
            SectionName = "PerformanceNarrative",
            UserId = Guid.NewGuid()
        });

        // Assert
        Assert.False(result.Success);
        Assert.Equal("API key not configured", result.ErrorMessage);
    }

    [Fact]
    public async Task GenerateSuggestion_WhenReportNotFound_ReturnsFailure()
    {
        _reportRepo.Setup(r => r.GetByIdWithSectionsAsync(It.IsAny<Guid>()))
                   .ReturnsAsync((Report?)null);

        var result = await CreateSut().GenerateSuggestionAsync(new SuggestionRequestDto
        {
            ReportId = Guid.NewGuid(),
            SectionName = "PerformanceNarrative",
            UserId = Guid.NewGuid()
        });

        Assert.False(result.Success);
        Assert.Equal("Report not found", result.ErrorMessage);
    }

    // ── Prompt quality ────────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateSuggestion_IncludesPreviousContentInPrompt()
    {
        // Arrange
        var grantId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var grant = new Grant { GrantId = grantId, GrantNumber = "GX-2024-00001", ProgramTypeCode = 60 };
        var report = new Report { ReportId = reportId, GrantId = grantId, Grant = grant, Sections = [] };
        string? capturedUserPrompt = null;

        _reportRepo.Setup(r => r.GetByIdWithSectionsAsync(reportId)).ReturnsAsync(report);
        _aiRepo.Setup(r => r.GetPreviousReportContentAsync(grantId, It.IsAny<string>()))
               .ReturnsAsync("PREVIOUS CONTENT MARKER");
        _aiRepo.Setup(r => r.FindExamplesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
               .ReturnsAsync([]);
        _aiRepo.Setup(r => r.LogUsageAsync(It.IsAny<AIUsageLog>())).ReturnsAsync(Guid.NewGuid());
        _openAI.Setup(o => o.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
               .Callback<string, string, int>((_, user, _) => capturedUserPrompt = user)
               .ReturnsAsync(new OpenAIResult(true, "Result", 100, 100, null));

        // Act
        await CreateSut().GenerateSuggestionAsync(new SuggestionRequestDto
        {
            ReportId = reportId,
            SectionName = "PerformanceNarrative",
            UserId = Guid.NewGuid()
        });

        // Assert
        Assert.NotNull(capturedUserPrompt);
        Assert.Contains("PREVIOUS CONTENT MARKER", capturedUserPrompt);
    }

    // ── Feedback ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordFeedback_CallsRepository()
    {
        var logId = Guid.NewGuid();
        _aiRepo.Setup(r => r.UpdateFeedbackAsync(logId, "Accepted", 5)).Returns(Task.CompletedTask);

        await CreateSut().RecordFeedbackAsync(new FeedbackRequestDto
        {
            LogId = logId,
            UserAction = "Accepted",
            UserRating = 5
        });

        _aiRepo.Verify(r => r.UpdateFeedbackAsync(logId, "Accepted", 5), Times.Once);
    }
}
