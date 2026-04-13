using GrantManagement.Core.Entities;

namespace GrantManagement.Core.Interfaces;

public interface IReportRepository
{
    Task<List<Report>> GetByGrantIdAsync(Guid grantId);
    Task<Report?> GetByIdAsync(Guid reportId);
    Task<Report?> GetByIdWithSectionsAsync(Guid reportId);
    Task<List<Report>> GetApprovedByGrantIdAsync(Guid grantId);
    Task<ReportSection?> GetSectionByIdAsync(Guid sectionId);
    Task UpdateSectionAsync(ReportSection section);
}
