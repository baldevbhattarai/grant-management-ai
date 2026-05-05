using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace GrantManagement.Services.AI;

public class QdrantVectorService(
    IConfiguration config,
    ILogger<QdrantVectorService> logger) : IVectorSearchService, IDocumentVectorService
{
    private const ulong VectorSize = 768; // nomic-embed-text dimension
    private QdrantClient? _client;
    private string _collectionName = "report_sections";
    private string _documentsCollectionName = "uploaded_documents";

    private QdrantClient GetClient()
    {
        if (_client is not null) return _client;

        var mode = config["AI:VectorSearch:Qdrant:Mode"] ?? "Local";
        _collectionName = config["AI:VectorSearch:Qdrant:Cloud:CollectionName"]
                       ?? config["AI:VectorSearch:Qdrant:Local:CollectionName"]
                       ?? "report_sections";
        _documentsCollectionName = config["AI:VectorSearch:Qdrant:DocumentsCollectionName"] ?? "uploaded_documents";

        if (mode.Equals("Cloud", StringComparison.OrdinalIgnoreCase))
        {
            var url = config["AI:VectorSearch:Qdrant:Cloud:Url"]
                ?? throw new InvalidOperationException("AI:VectorSearch:Qdrant:Cloud:Url is required for Cloud mode");
            var apiKey = config["AI:VectorSearch:Qdrant:Cloud:ApiKey"]
                ?? throw new InvalidOperationException("AI:VectorSearch:Qdrant:Cloud:ApiKey is required for Cloud mode");

            var uri = new Uri(url);
            _client = new QdrantClient(uri.Host, apiKey: apiKey, https: uri.Scheme == "https");
            logger.LogInformation("Qdrant connected: Cloud ({Host})", uri.Host);
        }
        else
        {
            var url = config["AI:VectorSearch:Qdrant:Local:Url"] ?? "http://localhost:6333";
            var uri = new Uri(url);
            _client = new QdrantClient(uri.Host, uri.Port);
            logger.LogInformation("Qdrant connected: Local ({Url})", url);
        }

        return _client;
    }

    public async Task EnsureCollectionAsync()
    {
        try
        {
            var client = GetClient();
            var collections = await client.ListCollectionsAsync();

            if (!collections.Any(c => c == _collectionName))
            {
                await client.CreateCollectionAsync(_collectionName,
                    new VectorParams { Size = VectorSize, Distance = Distance.Cosine });
                logger.LogInformation("Qdrant collection '{Collection}' created", _collectionName);
            }
            else
            {
                logger.LogInformation("Qdrant collection '{Collection}' already exists", _collectionName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure Qdrant collection — vector search will be unavailable");
        }
    }

    public Task UpsertSectionAsync(Guid sectionId, string text, Guid grantId, Guid reportId,
        int reportingYear, string reportingQuarter, string sectionName)
        => throw new NotSupportedException("Use VectorIndexingService.IndexSectionAsync directly");

    /// <summary>
    /// Upserts a single chunk. Chunk 0 uses the original sectionId as its point ID
    /// for backward compatibility; subsequent chunks use a derived deterministic UUID.
    /// All chunks carry parentSectionId in the payload for deduplication and orphan cleanup.
    /// </summary>
    public async Task UpsertChunkAsync(
        Guid sectionId,
        int chunkIndex,
        int totalChunks,
        float[] vector,
        Guid grantId,
        Guid reportId,
        int reportingYear,
        string reportingQuarter,
        string sectionName,
        string chunkText,
        string contentHash)
    {
        var client = GetClient();
        var pointId = ChunkPointId(sectionId, chunkIndex);

        var point = new PointStruct
        {
            Id = new PointId { Uuid = pointId.ToString() },
            Vectors = vector,
            Payload =
            {
                ["parentSectionId"] = sectionId.ToString(),
                ["chunkIndex"]      = chunkIndex,
                ["totalChunks"]     = totalChunks,
                ["grantId"]         = grantId.ToString(),
                ["reportId"]        = reportId.ToString(),
                ["reportingYear"]   = reportingYear,
                ["reportingQuarter"]= reportingQuarter,
                ["sectionName"]     = sectionName,
                ["responseText"]    = chunkText.Length > 1000 ? chunkText[..1000] : chunkText,
                ["contentHash"]     = contentHash
            }
        };

        await client.UpsertAsync(_collectionName, [point]);
    }

    // Kept for on-demand single-section re-index (backward compat — wraps UpsertChunkAsync)
    public async Task UpsertAsync(Guid sectionId, float[] vector, Guid grantId, Guid reportId,
        int reportingYear, string reportingQuarter, string sectionName, string responseText,
        string? contentHash = null)
    {
        await UpsertChunkAsync(sectionId, 0, 1, vector, grantId, reportId,
            reportingYear, reportingQuarter, sectionName, responseText, contentHash ?? string.Empty);
    }

    /// <summary>
    /// Returns parentSectionId → contentHash for all indexed points.
    /// Groups by parentSectionId so callers work with section-level granularity.
    /// </summary>
    public async Task<Dictionary<Guid, string>> GetAllContentHashesAsync()
    {
        var client = GetClient();
        var result = new Dictionary<Guid, string>();
        string? nextOffset = null;

        try
        {
            do
            {
                var offset = nextOffset is null ? null : new PointId { Uuid = nextOffset };
                var scrollResult = await client.ScrollAsync(
                    _collectionName,
                    limit: 250,
                    offset: offset,
                    payloadSelector: true,
                    vectorsSelector: false);

                foreach (var point in scrollResult.Result)
                {
                    // Resolve parentSectionId (new chunks) or fall back to point UUID (old single points)
                    var parentIdStr = point.Payload.TryGetValue("parentSectionId", out var pid)
                        ? pid.StringValue
                        : point.Id.Uuid;

                    if (!Guid.TryParse(parentIdStr, out var parentId)) continue;
                    if (result.ContainsKey(parentId)) continue; // already captured from another chunk

                    var hash = point.Payload.TryGetValue("contentHash", out var h) ? h.StringValue : string.Empty;
                    result[parentId] = hash;
                }

                nextOffset = scrollResult.NextPageOffset?.Uuid;
            }
            while (nextOffset is not null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch content hashes from Qdrant — will re-index all sections");
        }

        return result;
    }

    /// <summary>
    /// Deletes all Qdrant points whose parentSectionId is not in the valid set.
    /// Handles multi-chunk sections by deleting via payload filter.
    /// </summary>
    public async Task DeleteOrphanedAsync(IEnumerable<Guid> validSectionIds)
    {
        var client = GetClient();
        var valid = new HashSet<Guid>(validSectionIds);
        var allHashes = await GetAllContentHashesAsync();
        var orphanedIds = allHashes.Keys.Where(id => !valid.Contains(id)).ToList();

        if (orphanedIds.Count == 0) return;

        foreach (var parentId in orphanedIds)
        {
            var filter = new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "parentSectionId",
                            Match = new Match { Text = parentId.ToString() }
                        }
                    }
                }
            };
            await client.DeleteAsync(_collectionName, filter);
        }

        logger.LogInformation("Deleted {Count} orphaned section(s) from Qdrant", orphanedIds.Count);
    }

    public async Task BulkUpsertAsync(IEnumerable<VectorSectionDto> sections)
        => await Task.CompletedTask; // driven by VectorIndexingService

    public async Task<List<VectorSearchResult>> SearchAsync(
        float[] queryVector, Guid grantId, int topN = 3, float minScore = 0.5f)
    {
        try
        {
            var client = GetClient();

            var filter = new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "grantId",
                            Match = new Match { Text = grantId.ToString() }
                        }
                    }
                }
            };

            // Fetch more results than topN to allow deduplication across chunks
            var rawLimit = (ulong)(topN * 4);
            var results = await client.SearchAsync(
                _collectionName,
                (ReadOnlyMemory<float>)queryVector,
                filter: filter,
                limit: rawLimit,
                scoreThreshold: minScore,
                payloadSelector: true);

            // Deduplicate by parentSectionId — keep the highest-scoring chunk per section
            var seen = new Dictionary<Guid, VectorSearchResult>();

            foreach (var r in results)
            {
                var parentIdStr = r.Payload.TryGetValue("parentSectionId", out var pid)
                    ? pid.StringValue
                    : r.Id.Uuid;

                if (!Guid.TryParse(parentIdStr, out var parentId)) continue;

                Guid? reportId = null;
                if (r.Payload.TryGetValue("reportId", out var rid) &&
                    Guid.TryParse(rid.StringValue, out var parsedReportId))
                    reportId = parsedReportId;

                var result = new VectorSearchResult(
                    SectionId: parentId,
                    Score: r.Score,
                    ResponseText: r.Payload.TryGetValue("responseText", out var rt) ? rt.StringValue : string.Empty,
                    SectionName: r.Payload.TryGetValue("sectionName", out var sn) ? sn.StringValue : string.Empty,
                    ReportingYear: r.Payload.TryGetValue("reportingYear", out var ry) ? (int)ry.IntegerValue : 0,
                    ReportingQuarter: r.Payload.TryGetValue("reportingQuarter", out var rq) ? rq.StringValue : string.Empty,
                    ReportId: reportId);

                if (!seen.TryGetValue(parentId, out var existing) || r.Score > existing.Score)
                    seen[parentId] = result;
            }

            return seen.Values
                .OrderByDescending(r => r.Score)
                .Take(topN)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Qdrant search failed for grantId {GrantId}", grantId);
            return [];
        }
    }

    public async Task DeleteSectionAsync(Guid sectionId)
    {
        var client = GetClient();
        // Delete all chunks for this section via parentSectionId filter
        var filter = new Filter
        {
            Must =
            {
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "parentSectionId",
                        Match = new Match { Text = sectionId.ToString() }
                    }
                }
            }
        };
        await client.DeleteAsync(_collectionName, filter);
    }

    // ── Uploaded document vector methods ─────────────────────────────────────

    public async Task EnsureDocumentsCollectionAsync()
    {
        try
        {
            var client = GetClient();
            var collections = await client.ListCollectionsAsync();
            if (!collections.Any(c => c == _documentsCollectionName))
            {
                await client.CreateCollectionAsync(_documentsCollectionName,
                    new VectorParams { Size = VectorSize, Distance = Distance.Cosine });
                logger.LogInformation("Qdrant collection '{Collection}' created", _documentsCollectionName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure documents Qdrant collection");
        }
    }

    public async Task UpsertDocumentChunkAsync(
        Guid documentId, int chunkIndex, int totalChunks, float[] vector,
        Guid grantId, Guid userId, string fileName, string chunkText)
    {
        var client = GetClient();
        var pointId = ChunkPointId(documentId, chunkIndex);

        var point = new PointStruct
        {
            Id = new PointId { Uuid = pointId.ToString() },
            Vectors = vector,
            Payload =
            {
                ["documentId"]  = documentId.ToString(),
                ["chunkIndex"]  = chunkIndex,
                ["totalChunks"] = totalChunks,
                ["grantId"]     = grantId.ToString(),
                ["userId"]      = userId.ToString(),
                ["fileName"]    = fileName,
                ["chunkText"]   = chunkText.Length > 1000 ? chunkText[..1000] : chunkText
            }
        };

        await client.UpsertAsync(_documentsCollectionName, [point]);
    }

    public async Task DeleteDocumentAsync(Guid documentId)
    {
        var client = GetClient();
        var filter = new Filter
        {
            Must =
            {
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "documentId",
                        Match = new Match { Text = documentId.ToString() }
                    }
                }
            }
        };
        await client.DeleteAsync(_documentsCollectionName, filter);
    }

    public async Task<List<DocumentSearchResult>> SearchDocumentsAsync(
        float[] queryVector, Guid grantId, int topN = 3, float minScore = 0.4f)
    {
        try
        {
            var client = GetClient();
            var filter = new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "grantId",
                            Match = new Match { Text = grantId.ToString() }
                        }
                    }
                }
            };

            var results = await client.SearchAsync(
                _documentsCollectionName,
                (ReadOnlyMemory<float>)queryVector,
                filter: filter,
                limit: (ulong)topN,
                scoreThreshold: minScore,
                payloadSelector: true);

            return results.Select(r => new DocumentSearchResult(
                DocumentId: Guid.TryParse(
                    r.Payload.TryGetValue("documentId", out var did) ? did.StringValue : string.Empty,
                    out var docId) ? docId : Guid.Empty,
                Score: r.Score,
                ChunkText: r.Payload.TryGetValue("chunkText", out var ct) ? ct.StringValue : string.Empty,
                FileName: r.Payload.TryGetValue("fileName", out var fn) ? fn.StringValue : string.Empty,
                ChunkIndex: r.Payload.TryGetValue("chunkIndex", out var ci) ? (int)ci.IntegerValue : 0))
            .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Qdrant document search failed for grantId {GrantId}", grantId);
            return [];
        }
    }

    /// <summary>Deterministic chunk point ID. Chunk 0 reuses the original sectionId.</summary>
    private static Guid ChunkPointId(Guid sectionId, int chunkIndex)
    {
        if (chunkIndex == 0) return sectionId;
        var bytes = sectionId.ToByteArray();
        var idxBytes = BitConverter.GetBytes(chunkIndex);
        for (var i = 0; i < 4; i++) bytes[12 + i] ^= idxBytes[i];
        return new Guid(bytes);
    }
}
