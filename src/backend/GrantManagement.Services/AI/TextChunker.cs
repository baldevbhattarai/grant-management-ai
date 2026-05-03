namespace GrantManagement.Services.AI;

/// <summary>
/// Splits text into overlapping word-based windows so each chunk gets its own
/// embedding. Improves retrieval precision for long sections where a single
/// averaged vector dilutes relevant passages.
/// </summary>
public static class TextChunker
{
    /// <summary>
    /// Splits <paramref name="text"/> into overlapping chunks.
    /// </summary>
    /// <param name="text">Source text to chunk.</param>
    /// <param name="windowWords">Approximate words per chunk (default 90 ≈ 120 tokens).</param>
    /// <param name="overlapWords">Words shared between consecutive chunks (default 20).</param>
    public static IReadOnlyList<TextChunk> Chunk(string text, int windowWords = 90, int overlapWords = 20)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // If the text fits in a single window, return it as-is (no chunking needed)
        if (words.Length <= windowWords)
            return [new TextChunk(0, 1, text)];

        var chunks = new List<TextChunk>();
        var step = windowWords - overlapWords;
        if (step <= 0) step = windowWords; // guard against misconfiguration

        var index = 0;
        for (var start = 0; start < words.Length; start += step)
        {
            var chunkWords = words.Skip(start).Take(windowWords).ToArray();
            chunks.Add(new TextChunk(index++, 0 /* totalChunks set below */, string.Join(' ', chunkWords)));

            if (start + windowWords >= words.Length) break;
        }

        // Backfill totalChunks now that we know the final count
        var total = chunks.Count;
        return chunks.Select(c => c with { TotalChunks = total }).ToList();
    }
}

/// <param name="ChunkIndex">0-based index within the parent section.</param>
/// <param name="TotalChunks">Total number of chunks for the parent section.</param>
/// <param name="Text">The chunk text.</param>
public record TextChunk(int ChunkIndex, int TotalChunks, string Text);
