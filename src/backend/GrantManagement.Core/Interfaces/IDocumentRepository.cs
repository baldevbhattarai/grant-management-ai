using GrantManagement.Core.Entities;

namespace GrantManagement.Core.Interfaces;

public interface IDocumentRepository
{
    Task<AIUploadedDocument> AddAsync(AIUploadedDocument document);
    Task<List<AIUploadedDocument>> GetByGrantAsync(Guid grantId);
    Task<AIUploadedDocument?> GetByIdAsync(Guid documentId);
    Task UpdateChunkCountAsync(Guid documentId, int chunkCount);
    Task DeleteAsync(Guid documentId);
}
