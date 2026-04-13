namespace GrantManagement.Core.Interfaces;

public interface IEmbeddingService
{
    /// <summary>Generate a vector embedding for the given text.</summary>
    Task<float[]> EmbedAsync(string text);
}
