using GrantManagement.Core.Interfaces;
using GrantManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrantManagement.Services.AI;

/// <summary>
/// Background service that indexes all existing report sections into Qdrant on startup,
/// then stays available for on-demand re-indexing of individual sections.
/// </summary>
public class VectorIndexingService(
    IServiceScopeFactory scopeFactory,
    ILogger<VectorIndexingService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("VectorIndexingService starting — indexing report sections into Qdrant...");

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var vectorService = scope.ServiceProvider.GetRequiredService<QdrantVectorService>();

        // Ensure collection exists
        await vectorService.EnsureCollectionAsync();

        // Load all sections with non-null ResponseText
        var sections = await db.ReportSections
            .Include(s => s.Report)
            .Where(s => s.ResponseText != null && s.ResponseText.Length > 10)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Indexing {Count} report sections...", sections.Count);

        var indexed = 0;
        foreach (var section in sections)
        {
            if (cancellationToken.IsCancellationRequested) break;
            try
            {
                var vector = await embeddingService.EmbedAsync(section.ResponseText!);
                await vectorService.UpsertAsync(
                    section.SectionId,
                    vector,
                    section.Report.GrantId,
                    section.ReportId,
                    section.Report.ReportingYear,
                    section.Report.ReportingQuarter,
                    section.SectionName,
                    section.ResponseText!);
                indexed++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to index section {SectionId}", section.SectionId);
            }
        }

        logger.LogInformation("Qdrant indexing complete — {Indexed}/{Total} sections indexed", indexed, sections.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Index or re-index a single section (called after save).</summary>
    public static async Task IndexSectionAsync(
        IEmbeddingService embeddingService,
        IVectorSearchService vectorService,
        Guid sectionId, string responseText, Guid grantId, Guid reportId,
        int reportingYear, string reportingQuarter, string sectionName)
    {
        var vector = await embeddingService.EmbedAsync(responseText);
        // Cast to QdrantVectorService to access UpsertAsync (not on interface — keeps interface minimal)
        if (vectorService is QdrantVectorService qdrant)
            await qdrant.UpsertAsync(sectionId, vector, grantId, reportId,
                reportingYear, reportingQuarter, sectionName, responseText);
    }
}
