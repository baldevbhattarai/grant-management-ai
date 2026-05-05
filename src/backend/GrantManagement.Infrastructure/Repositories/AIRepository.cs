using GrantManagement.Core.DTOs;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using GrantManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrantManagement.Infrastructure.Repositories;

public class AIRepository(ApplicationDbContext db) : IAIRepository
{
    public async Task<List<AIApprovedContent>> FindExamplesAsync(
        int programTypeCode, string sectionName, Guid excludeGrantId, int topN = 3)
    {
        return await db.AIApprovedContent
            .Where(c =>
                c.ProgramTypeCode == programTypeCode &&
                c.SectionName == sectionName &&
                c.GrantId != excludeGrantId &&
                c.ReviewerRating >= 4)
            .OrderByDescending(c => c.ReviewerRating)
            .ThenByDescending(c => c.ApprovalDate)
            .Take(topN)
            .ToListAsync();
    }

    public async Task<string?> GetPreviousReportContentAsync(Guid grantId, string sectionName)
    {
        return await db.ReportSections
            .Include(s => s.Report)
            .Where(s =>
                s.Report.GrantId == grantId &&
                s.SectionName == sectionName &&
                s.Report.Status == "Approved" &&
                s.ResponseText != null)
            .OrderByDescending(s => s.Report.ReportingYear)
            .ThenByDescending(s => s.Report.ReportingQuarter)
            .Select(s => s.ResponseText)
            .FirstOrDefaultAsync();
    }

    public async Task<Guid> LogUsageAsync(AIUsageLog log)
    {
        db.AIUsageLogs.Add(log);
        await db.SaveChangesAsync();
        return log.LogId;
    }

    public async Task UpdateFeedbackAsync(Guid logId, string userAction, int? userRating)
    {
        var log = await db.AIUsageLogs.FindAsync(logId);
        if (log is not null)
        {
            log.UserAction = userAction;
            log.UserRating = userRating;
            await db.SaveChangesAsync();
        }
    }

    public async Task<AIUsageLog?> GetUsageLogAsync(Guid logId) =>
        await db.AIUsageLogs.FindAsync(logId);

    public async Task AddApprovedContentAsync(AIApprovedContent content)
    {
        db.AIApprovedContent.Add(content);
        await db.SaveChangesAsync();
    }

    public async Task<List<ReportSection>> SearchSectionsAsync(Guid grantId, string keyword, int topN = 5)
    {
        var lower = keyword.ToLower();
        return await db.ReportSections
            .Include(s => s.Report)
            .Where(s =>
                s.Report.GrantId == grantId &&
                s.ResponseText != null &&
                s.ResponseText.ToLower().Contains(lower))
            .OrderByDescending(s => s.Report.ReportingYear)
            .ThenByDescending(s => s.Report.ReportingQuarter)
            .Take(topN)
            .ToListAsync();
    }

    public async Task<List<ReportSection>> GetStructuredDataAsync(Guid grantId, string sectionNameContains, int topN = 5)
    {
        var lower = sectionNameContains.ToLower();
        return await db.ReportSections
            .Include(s => s.Report)
            .Where(s =>
                s.Report.GrantId == grantId &&
                s.SectionName.ToLower().Contains(lower) &&
                (s.ResponseNumber != null || s.ResponseSingle != null || s.ResponseText != null))
            .OrderByDescending(s => s.Report.ReportingYear)
            .ThenByDescending(s => s.Report.ReportingQuarter)
            .Take(topN)
            .ToListAsync();
    }

    public async Task<List<Report>> GetRecentReportsAsync(Guid grantId, int topN = 4)
    {
        return await db.Reports
            .Where(r => r.GrantId == grantId)
            .OrderByDescending(r => r.ReportingYear)
            .ThenByDescending(r => r.ReportingQuarter)
            .Take(topN)
            .ToListAsync();
    }

    public async Task<UsageSummaryDto> GetUsageSummaryAsync(int days = 30)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var logs = await db.AIUsageLogs
            .Where(l => l.CreatedDate >= since)
            .ToListAsync();

        if (logs.Count == 0)
            return new UsageSummaryDto();

        var totalRequests = logs.Count;
        var successful = logs.Count(l => l.Success);
        var totalTokens = logs.Sum(l => (long)(l.TotalTokens ?? 0));
        var totalCost = logs.Sum(l => l.EstimatedCost ?? 0m);
        var avgResponseMs = logs.Where(l => l.ResponseTimeMs.HasValue).Select(l => (double)l.ResponseTimeMs!.Value)
            .DefaultIfEmpty(0).Average();

        var byFeature = logs
            .GroupBy(l => l.FeatureType)
            .Select(g => new UsageByFeatureDto(
                g.Key,
                g.Count(),
                g.Sum(l => (long)(l.TotalTokens ?? 0)),
                g.Sum(l => l.EstimatedCost ?? 0m),
                g.Where(l => l.ResponseTimeMs.HasValue).Select(l => (double)l.ResponseTimeMs!.Value).DefaultIfEmpty(0).Average(),
                g.Count() == 0 ? 0 : (double)g.Count(l => l.Success) / g.Count() * 100))
            .OrderByDescending(f => f.Requests)
            .ToList();

        var byDay = logs
            .GroupBy(l => l.CreatedDate.Date)
            .Select(g => new UsageByDayDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Count(),
                g.Sum(l => (long)(l.TotalTokens ?? 0)),
                g.Sum(l => l.EstimatedCost ?? 0m)))
            .OrderBy(d => d.Date)
            .ToList();

        var topGrants = logs
            .GroupBy(l => l.GrantId)
            .Select(g => new { GrantId = g.Key, Requests = g.Count(), Tokens = g.Sum(l => (long)(l.TotalTokens ?? 0)), Cost = g.Sum(l => l.EstimatedCost ?? 0m) })
            .OrderByDescending(g => g.Requests)
            .Take(5)
            .ToList();

        var grantNumbers = await db.Grants
            .Where(g => topGrants.Select(t => t.GrantId).Contains(g.GrantId))
            .ToDictionaryAsync(g => g.GrantId, g => g.GrantNumber);

        var topGrantDtos = topGrants.Select(g => new TopGrantUsageDto(
            g.GrantId, grantNumbers.GetValueOrDefault(g.GrantId, "Unknown"), g.Requests, g.Tokens, g.Cost)).ToList();

        return new UsageSummaryDto
        {
            TotalRequests = totalRequests,
            SuccessfulRequests = successful,
            TotalTokens = totalTokens,
            TotalCost = totalCost,
            AvgResponseTimeMs = avgResponseMs,
            SuccessRate = totalRequests == 0 ? 0 : (double)successful / totalRequests * 100,
            ByFeature = byFeature,
            ByDay = byDay,
            TopGrants = topGrantDtos
        };
    }
}
