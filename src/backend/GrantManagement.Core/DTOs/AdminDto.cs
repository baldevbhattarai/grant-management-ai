namespace GrantManagement.Core.DTOs;

public record UsageSummaryDto
{
    public int TotalRequests { get; init; }
    public int SuccessfulRequests { get; init; }
    public long TotalTokens { get; init; }
    public decimal TotalCost { get; init; }
    public double AvgResponseTimeMs { get; init; }
    public double SuccessRate { get; init; }
    public List<UsageByFeatureDto> ByFeature { get; init; } = [];
    public List<UsageByDayDto> ByDay { get; init; } = [];
    public List<TopGrantUsageDto> TopGrants { get; init; } = [];
}

public record UsageByFeatureDto(
    string FeatureType,
    int Requests,
    long Tokens,
    decimal Cost,
    double AvgResponseTimeMs,
    double SuccessRate);

public record UsageByDayDto(string Date, int Requests, long Tokens, decimal Cost);

public record TopGrantUsageDto(Guid GrantId, string GrantNumber, int Requests, long Tokens, decimal Cost);
