using System.Security.Cryptography;
using System.Text;
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

        logger.LogInformation("Indexing {Count} report sections (dirty-check enabled)...", sections.Count);

        // Fetch existing hashes to skip unchanged sections
        var existingHashes = await vectorService.GetAllContentHashesAsync();
        var validSectionIds = sections.Select(s => s.SectionId).ToHashSet();

        var indexed = 0;
        var skipped = 0;
        foreach (var section in sections)
        {
            if (cancellationToken.IsCancellationRequested) break;
            try
            {
                var hash = ComputeHash(section.ResponseText!);

                // Skip if hash matches — content unchanged
                if (existingHashes.TryGetValue(section.SectionId, out var storedHash) && storedHash == hash)
                {
                    skipped++;
                    continue;
                }

                await IndexChunksAsync(embeddingService, vectorService, section.SectionId,
                    section.ResponseText!, section.Report.GrantId, section.ReportId,
                    section.Report.ReportingYear, section.Report.ReportingQuarter,
                    section.SectionName, hash);
                indexed++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to index section {SectionId}", section.SectionId);
            }
        }

        // Remove Qdrant points for sections that no longer exist in SQL
        await vectorService.DeleteOrphanedAsync(validSectionIds);

        logger.LogInformation(
            "Qdrant indexing complete — {Indexed} re-indexed, {Skipped} unchanged, {Total} total",
            indexed, skipped, sections.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Index or re-index a single section on-demand (called after save).</summary>
    public static async Task IndexSectionAsync(
        IEmbeddingService embeddingService,
        IVectorSearchService vectorService,
        Guid sectionId, string responseText, Guid grantId, Guid reportId,
        int reportingYear, string reportingQuarter, string sectionName)
    {
        if (vectorService is not QdrantVectorService qdrant) return;
        var hash = ComputeHash(responseText);
        await IndexChunksAsync(embeddingService, qdrant, sectionId, responseText,
            grantId, reportId, reportingYear, reportingQuarter, sectionName, hash);
    }

    /// <summary>
    /// Chunks the section text and upserts each chunk into Qdrant.
    /// Short sections produce a single chunk; long sections produce 2–6 overlapping chunks.
    /// </summary>
    private static async Task IndexChunksAsync(
        IEmbeddingService embeddingService,
        QdrantVectorService qdrant,
        Guid sectionId, string responseText, Guid grantId, Guid reportId,
        int reportingYear, string reportingQuarter, string sectionName, string hash)
    {
        var chunks = TextChunker.Chunk(responseText);
        foreach (var chunk in chunks)
        {
            var vector = await embeddingService.EmbedAsync(chunk.Text);
            await qdrant.UpsertChunkAsync(
                sectionId, chunk.ChunkIndex, chunk.TotalChunks,
                vector, grantId, reportId,
                reportingYear, reportingQuarter, sectionName,
                chunk.Text, hash);
        }
    }

    private static string ComputeHash(string text)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }
}
