using System.Net.Http.Json;
using System.Text.Json;
using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrantManagement.Services.AI;

public class EmbeddingService(
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<EmbeddingService> logger) : IEmbeddingService
{
    public async Task<float[]> EmbedAsync(string text)
    {
        var baseUrl = config["AI:Ollama:BaseUrl"] ?? "http://localhost:11434/v1";
        var model = config["AI:Ollama:EmbeddingModel"] ?? "nomic-embed-text";

        // Ollama embeddings endpoint (non-OpenAI-compat path)
        var url = $"{baseUrl.Replace("/v1", "").TrimEnd('/')}/api/embeddings";

        var client = httpClientFactory.CreateClient("openai");
        client.DefaultRequestHeaders.Clear();

        var payload = new { model, prompt = text };

        try
        {
            var response = await client.PostAsJsonAsync(url, payload);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Ollama embedding error {Status}: {Body}", response.StatusCode, body);
                throw new InvalidOperationException($"Embedding failed: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(body);
            var embedding = doc.RootElement.GetProperty("embedding");
            return embedding.EnumerateArray().Select(e => e.GetSingle()).ToArray();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(ex, "Error calling Ollama embeddings API");
            throw;
        }
    }
}
