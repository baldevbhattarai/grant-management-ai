using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using GrantManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrantManagement.Infrastructure.Repositories;

public class ReportRepository(ApplicationDbContext db) : IReportRepository
{
    public async Task<List<Report>> GetByGrantIdAsync(Guid grantId)
    {
        return await db.Reports
            .Where(r => r.GrantId == grantId)
            .OrderByDescending(r => r.ReportingYear)
            .ThenBy(r => r.ReportingQuarter)
            .ToListAsync();
    }

    public async Task<Report?> GetByIdAsync(Guid reportId)
    {
        return await db.Reports.FindAsync(reportId);
    }

    public async Task<Report?> GetByIdWithSectionsAsync(Guid reportId)
    {
        return await db.Reports
            .Include(r => r.Grant)
            .Include(r => r.Sections.OrderBy(s => s.SectionOrder))
            .FirstOrDefaultAsync(r => r.ReportId == reportId);
    }

    public async Task<List<Report>> GetApprovedByGrantIdAsync(Guid grantId)
    {
        return await db.Reports
            .Include(r => r.Sections)
            .Where(r => r.GrantId == grantId && r.Status == "Approved")
            .OrderByDescending(r => r.ReportingYear)
            .ThenByDescending(r => r.ReportingQuarter)
            .ToListAsync();
    }

    public async Task<ReportSection?> GetSectionByIdAsync(Guid sectionId)
    {
        return await db.ReportSections.FindAsync(sectionId);
    }

    public async Task UpdateSectionAsync(ReportSection section)
    {
        section.ModifiedDate = DateTime.UtcNow;
        db.ReportSections.Update(section);
        await db.SaveChangesAsync();
    }
}
