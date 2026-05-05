namespace GrantManagement.Core.Interfaces;

public interface IDocumentVectorService
{
    Task EnsureDocumentsCollectionAsync();
    Task UpsertDocumentChunkAsync(Guid documentId, int chunkIndex, int totalChunks,
        float[] vector, Guid grantId, Guid userId, string fileName, string chunkText);
    Task DeleteDocumentAsync(Guid documentId);
}
