using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrantManagement.Services.AI;

/// <summary>
/// Re-ranks documents using the Cohere Rerank API (rerank-english-v3.0).
/// Falls back to original order gracefully when no API key is configured
/// or the API call fails — the chatbot degrades to RRF-only ranking.
/// Free tier: 1,000 reranks/month.
/// </summary>
public class CohereRerankService(
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<CohereRerankService> logger) : IRerankService
{
    private const string RerankModel = "rerank-english-v3.0";
    private const string CohereRerankUrl = "https://api.cohere.com/v1/rerank";

    private string? ApiKey => config["AI:Rerank:Cohere:ApiKey"];

    public bool IsEnabled => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<List<int>> RerankAsync(string query, IReadOnlyList<string> documents, int topN = 5)
    {
        if (!IsEnabled || documents.Count == 0)
            return Enumerable.Range(0, Math.Min(documents.Count, topN)).ToList();

        try
        {
            var http = httpClientFactory.CreateClient("cohere");
            using var request = new HttpRequestMessage(HttpMethod.Post, CohereRerankUrl);
            request.Headers.Add("Authorization", $"Bearer {ApiKey}");

            var body = new
            {
                model = RerankModel,
                query,
                documents = documents.Select(d => d.Length > 512 ? d[..512] : d).ToList(),
                top_n = Math.Min(topN, documents.Count),
                return_documents = false
            };

            request.Content = JsonContent.Create(body);
            var response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Cohere rerank returned {Status} — using original order", response.StatusCode);
                return Enumerable.Range(0, Math.Min(documents.Count, topN)).ToList();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CohereRerankResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Results is null or { Count: 0 })
                return Enumerable.Range(0, Math.Min(documents.Count, topN)).ToList();

            return result.Results
                .OrderByDescending(r => r.RelevanceScore)
                .Take(topN)
                .Select(r => r.Index)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cohere rerank failed — using original order");
            return Enumerable.Range(0, Math.Min(documents.Count, topN)).ToList();
        }
    }
}

file record CohereRerankResponse(
    [property: JsonPropertyName("results")] List<CohereRerankResult> Results);

file record CohereRerankResult(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("relevance_score")] float RelevanceScore);
