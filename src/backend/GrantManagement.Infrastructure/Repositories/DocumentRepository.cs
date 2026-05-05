using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using GrantManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrantManagement.Infrastructure.Repositories;

public class DocumentRepository(ApplicationDbContext db) : IDocumentRepository
{
    public async Task<AIUploadedDocument> AddAsync(AIUploadedDocument document)
    {
        db.AIUploadedDocuments.Add(document);
        await db.SaveChangesAsync();
        return document;
    }

    public async Task<List<AIUploadedDocument>> GetByGrantAsync(Guid grantId)
        => await db.AIUploadedDocuments
            .Where(d => d.GrantId == grantId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

    public async Task<AIUploadedDocument?> GetByIdAsync(Guid documentId)
        => await db.AIUploadedDocuments.FindAsync(documentId);

    public async Task UpdateChunkCountAsync(Guid documentId, int chunkCount)
    {
        var doc = await db.AIUploadedDocuments.FindAsync(documentId);
        if (doc is null) return;
        doc.ChunkCount = chunkCount;
        doc.IsIndexed = true;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid documentId)
    {
        var doc = await db.AIUploadedDocuments.FindAsync(documentId);
        if (doc is null) return;
        db.AIUploadedDocuments.Remove(doc);
        await db.SaveChangesAsync();
    }
}
