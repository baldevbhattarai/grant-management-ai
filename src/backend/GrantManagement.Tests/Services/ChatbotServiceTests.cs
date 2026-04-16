using GrantManagement.Core.DTOs;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using GrantManagement.Services.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GrantManagement.Tests.Services;

public class ChatbotServiceTests
{
    private readonly Mock<IGrantRepository> _grantRepo = new();
    private readonly Mock<IAIRepository> _aiRepo = new();
    private readonly Mock<IOpenAIService> _openAI = new();
    private readonly Mock<IEmbeddingService> _embedding = new();
    private readonly Mock<IVectorSearchService> _vectorSearch = new();

    public ChatbotServiceTests()
    {
        // Vector search returns empty by default — tests fall back to SQL LIKE keyword path
        _embedding.Setup(e => e.EmbedAsync(It.IsAny<string>())).ReturnsAsync(new float[768]);
        _vectorSearch.Setup(v => v.SearchAsync(It.IsAny<float[]>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<float>()))
                     .ReturnsAsync([]);
    }

    private ChatbotService CreateSut() =>
        new(_grantRepo.Object, _aiRepo.Object, _openAI.Object,
            _embedding.Object, _vectorSearch.Object,
            NullLogger<ChatbotService>.Instance);

    [Fact]
    public async Task Ask_WhenGrantNotFound_ReturnsFailure()
    {
        _grantRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Grant?)null);

        var result = await CreateSut().AskAsync(new ChatRequestDto
        {
            UserId = Guid.NewGuid(),
            GrantId = Guid.NewGuid(),
            Question = "How many patients?"
        });

        Assert.False(result.Success);
        Assert.Equal("Grant not found", result.ErrorMessage);
    }

    [Fact]
    public async Task Ask_WithValidGrant_ReturnsAnswer()
    {
        var grantId = Guid.NewGuid();
        var grant = new Grant
        {
            GrantId = grantId,
            GrantNumber = "GX-2024-00001",
            GrantType = "C16",
            ProgramName = "Community Health"
        };

        _grantRepo.Setup(r => r.GetByIdAsync(grantId)).ReturnsAsync(grant);
        _aiRepo.Setup(r => r.SearchSectionsAsync(grantId, It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync([]);
        _aiRepo.Setup(r => r.LogUsageAsync(It.IsAny<AIUsageLog>())).ReturnsAsync(Guid.NewGuid());
        _openAI.Setup(o => o.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync(new OpenAIResult(true, "You served 2,850 patients in Q1 2024.", 200, 80, null));

        var result = await CreateSut().AskAsync(new ChatRequestDto
        {
            UserId = Guid.NewGuid(),
            GrantId = grantId,
            Question = "How many patients did I serve in Q1 2024?"
        });

        Assert.True(result.Success);
        Assert.Equal("You served 2,850 patients in Q1 2024.", result.Answer);
    }

    [Fact]
    public async Task Ask_WhenOpenAIFails_ReturnsFailure()
    {
        var grantId = Guid.NewGuid();
        var grant = new Grant { GrantId = grantId, GrantNumber = "GX-2024-00001", ProgramName = "Health", GrantType = "C16" };

        _grantRepo.Setup(r => r.GetByIdAsync(grantId)).ReturnsAsync(grant);
        _aiRepo.Setup(r => r.SearchSectionsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync([]);
        _aiRepo.Setup(r => r.LogUsageAsync(It.IsAny<AIUsageLog>())).ReturnsAsync(Guid.NewGuid());
        _openAI.Setup(o => o.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync(new OpenAIResult(false, null, 0, 0, "Timeout"));

        var result = await CreateSut().AskAsync(new ChatRequestDto
        {
            UserId = Guid.NewGuid(),
            GrantId = grantId,
            Question = "Anything?"
        });

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Ask_SourcesReturnedInResponse()
    {
        var grantId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var grant = new Grant { GrantId = grantId, GrantNumber = "GX-2024-00001", ProgramName = "Health", GrantType = "C16" };

        var section = new ReportSection
        {
            SectionId = Guid.NewGuid(),
            SectionName = "PerformanceNarrative",
            ResponseText = "We served 2850 patients with telehealth services.",
            Report = new Report
            {
                ReportId = reportId, GrantId = grantId,
                ReportingYear = 2024, ReportingQuarter = "Q1",
                Grant = grant
            }
        };

        _grantRepo.Setup(r => r.GetByIdAsync(grantId)).ReturnsAsync(grant);
        _aiRepo.Setup(r => r.SearchSectionsAsync(grantId, It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync([section]);
        _aiRepo.Setup(r => r.LogUsageAsync(It.IsAny<AIUsageLog>())).ReturnsAsync(Guid.NewGuid());
        _openAI.Setup(o => o.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
               .ReturnsAsync(new OpenAIResult(true, "Answer with sources", 200, 80, null));

        var result = await CreateSut().AskAsync(new ChatRequestDto
        {
            UserId = Guid.NewGuid(),
            GrantId = grantId,
            Question = "telehealth patients"
        });

        Assert.True(result.Success);
        Assert.NotEmpty(result.Sources);
        Assert.Equal("2024 Q1", result.Sources[0].ReportPeriod);
        Assert.Equal("PerformanceNarrative", result.Sources[0].SectionName);
    }
}
