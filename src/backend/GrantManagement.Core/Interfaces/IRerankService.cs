namespace GrantManagement.Core.Interfaces;

/// <summary>
/// Re-ranks retrieved documents against a query using a cross-encoder model.
/// More accurate than bi-encoder cosine similarity but slower — applied after
/// initial retrieval to re-order the top-K results before passing to the LLM.
/// </summary>
public interface IRerankService
{
    /// <summary>
    /// Re-ranks <paramref name="documents"/> by relevance to <paramref name="query"/>.
    /// Returns indices into the original list, ordered from most to least relevant.
    /// Returns the original order if re-ranking is unavailable.
    /// </summary>
    Task<List<int>> RerankAsync(string query, IReadOnlyList<string> documents, int topN = 5);

    /// <summary>Whether this implementation is backed by a real model (false = passthrough).</summary>
    bool IsEnabled { get; }
}
