using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace GrantManagement.Services.AI;

public class QdrantVectorService(
    IConfiguration config,
    ILogger<QdrantVectorService> logger) : IVectorSearchService
{
    private const ulong VectorSize = 768; // nomic-embed-text dimension
    private QdrantClient? _client;
    private string _collectionName = "report_sections";

    private QdrantClient GetClient()
    {
        if (_client is not null) return _client;

        var mode = config["AI:VectorSearch:Qdrant:Mode"] ?? "Local";
        _collectionName = config["AI:VectorSearch:Qdrant:Cloud:CollectionName"]
                       ?? config["AI:VectorSearch:Qdrant:Local:CollectionName"]
                       ?? "report_sections";

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
                    new VectorParams
                    {
                        Size = VectorSize,
                        Distance = Distance.Cosine
                    });
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
    {
        // Embedding is done by the caller — use VectorIndexingService.IndexSectionAsync instead
        throw new NotSupportedException("Use VectorIndexingService.IndexSectionAsync directly");
    }

    public async Task UpsertAsync(Guid sectionId, float[] vector, Guid grantId, Guid reportId,
        int reportingYear, string reportingQuarter, string sectionName, string responseText)
    {
        var client = GetClient();
        var point = new PointStruct
        {
            Id = new PointId { Uuid = sectionId.ToString() },
            Vectors = vector,
            Payload =
            {
                ["grantId"] = grantId.ToString(),
                ["reportId"] = reportId.ToString(),
                ["reportingYear"] = reportingYear,
                ["reportingQuarter"] = reportingQuarter,
                ["sectionName"] = sectionName,
                ["responseText"] = responseText.Length > 1000 ? responseText[..1000] : responseText
            }
        };

        await client.UpsertAsync(_collectionName, [point]);
    }

    public async Task BulkUpsertAsync(IEnumerable<VectorSectionDto> sections)
    {
        // Bulk upsert is driven by VectorIndexingService which handles embeddings
        await Task.CompletedTask;
    }

    public async Task<List<VectorSearchResult>> SearchAsync(float[] queryVector, Guid grantId, int topN = 3, float minScore = 0.5f)
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
                _collectionName,
                (ReadOnlyMemory<float>)queryVector,
                filter: filter,
                limit: (ulong)topN,
                scoreThreshold: minScore,
                payloadSelector: true);

            return results.Select(r => new VectorSearchResult(
                SectionId: Guid.Parse(r.Id.Uuid),
                Score: r.Score,
                ResponseText: r.Payload.TryGetValue("responseText", out var rt) ? rt.StringValue : string.Empty,
                SectionName: r.Payload.TryGetValue("sectionName", out var sn) ? sn.StringValue : string.Empty,
                ReportingYear: r.Payload.TryGetValue("reportingYear", out var ry) ? (int)ry.IntegerValue : 0,
                ReportingQuarter: r.Payload.TryGetValue("reportingQuarter", out var rq) ? rq.StringValue : string.Empty
            )).ToList();
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
        await client.DeleteAsync(_collectionName, sectionId);
    }
}
