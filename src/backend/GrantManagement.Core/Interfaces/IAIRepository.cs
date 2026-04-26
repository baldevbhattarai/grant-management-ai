using GrantManagement.Core.Entities;

namespace GrantManagement.Core.Interfaces;

public interface IAIRepository
{
    Task<List<AIApprovedContent>> FindExamplesAsync(int programTypeCode, string sectionName, Guid excludeGrantId, int topN = 3);
    Task<string?> GetPreviousReportContentAsync(Guid grantId, string sectionName);
    Task<Guid> LogUsageAsync(AIUsageLog log);
    Task UpdateFeedbackAsync(Guid logId, string userAction, int? userRating);
    Task<List<ReportSection>> SearchSectionsAsync(Guid grantId, string keyword, int topN = 5);

    /// <summary>Returns numeric and single-select report sections matching a section name pattern for a grant,
    /// ordered most recent first. Used for structured data intent queries (e.g. "how many patients?").</summary>
    Task<List<ReportSection>> GetStructuredDataAsync(Guid grantId, string sectionNameContains, int topN = 5);

    /// <summary>Returns the most recent reports for a grant regardless of section.</summary>
    Task<List<Report>> GetRecentReportsAsync(Guid grantId, int topN = 4);
}
