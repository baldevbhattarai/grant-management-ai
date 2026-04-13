using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrantManagement.Services.AI;

public interface IOpenAIService
{
    Task<OpenAIResult> CompleteAsync(string systemPrompt, string userPrompt, int maxTokens = 1500);
}

public record OpenAIResult(bool Success, string? Content, int PromptTokens, int CompletionTokens, string? Error);

public class OpenAIService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<OpenAIService> logger) : IOpenAIService
{
    public async Task<OpenAIResult> CompleteAsync(string systemPrompt, string userPrompt, int maxTokens = 1500)
    {
        var provider = config["AI:Provider"] ?? "Ollama";

        return provider.Equals("Claude", StringComparison.OrdinalIgnoreCase)
            ? await CallClaude(systemPrompt, userPrompt, maxTokens)
            : await CallOllama(systemPrompt, userPrompt, maxTokens);
    }

    // ── Ollama (OpenAI-compatible) ────────────────────────────────────────────
    private async Task<OpenAIResult> CallOllama(string systemPrompt, string userPrompt, int maxTokens)
    {
        var baseUrl = config["AI:Ollama:BaseUrl"] ?? "http://localhost:11434/v1";
        var model = config["AI:Ollama:Model"] ?? "qwen2.5-coder:7b";
        var url = $"{baseUrl.TrimEnd('/')}/chat/completions";

        var client = httpClientFactory.CreateClient("openai");
        client.DefaultRequestHeaders.Clear();

        var payload = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            max_tokens = maxTokens,
            temperature = 0.7,
            stream = false
        };

        try
        {
            var response = await client.PostAsJsonAsync(url, payload);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Ollama API error {Status}: {Body}", response.StatusCode, body);
                return new OpenAIResult(false, null, 0, 0, $"Ollama error: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var content = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            var promptTokens = 0;
            var completionTokens = 0;
            if (root.TryGetProperty("usage", out var usage))
            {
                usage.TryGetProperty("prompt_tokens", out var pt);
                usage.TryGetProperty("completion_tokens", out var ct);
                promptTokens = pt.ValueKind == JsonValueKind.Number ? pt.GetInt32() : 0;
                completionTokens = ct.ValueKind == JsonValueKind.Number ? ct.GetInt32() : 0;
            }

            return new OpenAIResult(true, content.Trim(), promptTokens, completionTokens, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling Ollama API");
            return new OpenAIResult(false, null, 0, 0, ex.Message);
        }
    }

    // ── Claude (Anthropic) ────────────────────────────────────────────────────
    private async Task<OpenAIResult> CallClaude(string systemPrompt, string userPrompt, int maxTokens)
    {
        var apiKey = config["AI:Claude:ApiKey"];
        var model = config["AI:Claude:Model"] ?? "claude-haiku-4-5-20251001";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Claude API key not configured");
            return new OpenAIResult(false, null, 0, 0, "Claude API key not configured");
        }

        var client = httpClientFactory.CreateClient("openai");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var payload = new
        {
            model,
            max_tokens = maxTokens,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        try
        {
            var response = await client.PostAsJsonAsync("https://api.anthropic.com/v1/messages", payload);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Claude API error {Status}: {Body}", response.StatusCode, body);
                return new OpenAIResult(false, null, 0, 0, $"Claude error: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var content = root
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            var usage = root.GetProperty("usage");
            var promptTokens = usage.GetProperty("input_tokens").GetInt32();
            var completionTokens = usage.GetProperty("output_tokens").GetInt32();

            return new OpenAIResult(true, content.Trim(), promptTokens, completionTokens, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling Claude API");
            return new OpenAIResult(false, null, 0, 0, ex.Message);
        }
    }
}
