using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using GrantManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrantManagement.Infrastructure.Repositories;

public class GrantRepository(ApplicationDbContext db) : IGrantRepository
{
    public async Task<List<Grant>> GetByUserIdAsync(Guid userId)
    {
        return await db.Grants
            .Include(g => g.User)
            .Where(g => g.UserId == userId && g.Status == "Active")
            .OrderBy(g => g.GrantNumber)
            .ToListAsync();
    }

    public async Task<Grant?> GetByIdAsync(Guid grantId)
    {
        return await db.Grants
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.GrantId == grantId);
    }

    public async Task<Grant?> GetByIdWithReportsAsync(Guid grantId)
    {
        return await db.Grants
            .Include(g => g.User)
            .Include(g => g.Reports.OrderByDescending(r => r.ReportingYear).ThenBy(r => r.ReportingQuarter))
            .FirstOrDefaultAsync(g => g.GrantId == grantId);
    }
}
