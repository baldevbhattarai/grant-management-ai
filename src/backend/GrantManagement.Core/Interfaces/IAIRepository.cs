using GrantManagement.Core.Entities;

namespace GrantManagement.Core.Interfaces;

public interface IAIRepository
{
    Task<List<AIApprovedContent>> FindExamplesAsync(int programTypeCode, string sectionName, Guid excludeGrantId, int topN = 3);
    Task<string?> GetPreviousReportContentAsync(Guid grantId, string sectionName);
    Task<Guid> LogUsageAsync(AIUsageLog log);
    Task UpdateFeedbackAsync(Guid logId, string userAction, int? userRating);
    Task<List<ReportSection>> SearchSectionsAsync(Guid grantId, string keyword, int topN = 5);
}
