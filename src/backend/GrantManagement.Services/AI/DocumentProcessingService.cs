using DocumentFormat.OpenXml.Packaging;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace GrantManagement.Services.AI;

public interface IDocumentProcessingService
{
    Task<AIUploadedDocument> ProcessAndIndexAsync(Stream fileStream, string fileName,
        string contentType, long fileSizeBytes, Guid grantId, Guid userId);
    Task DeleteAsync(Guid documentId);
}

public class DocumentProcessingService(
    IDocumentRepository docRepo,
    IDocumentVectorService docVectorService,
    IEmbeddingService embeddingService,
    ILogger<DocumentProcessingService> logger) : IDocumentProcessingService
{
    private static readonly string[] AllowedExtensions = [".pdf", ".docx", ".txt"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public async Task<AIUploadedDocument> ProcessAndIndexAsync(
        Stream fileStream, string fileName, string contentType, long fileSizeBytes, Guid grantId, Guid userId)
    {
        if (fileSizeBytes > MaxFileSizeBytes)
            throw new InvalidOperationException("File exceeds the maximum size of 10MB");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"Unsupported file type '{ext}'. Allowed: {string.Join(", ", AllowedExtensions)}");

        await docVectorService.EnsureDocumentsCollectionAsync();

        var text = await ExtractTextAsync(fileStream, ext);
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("No text could be extracted from the document");

        var chunks = TextChunker.Chunk(text);

        var document = new AIUploadedDocument
        {
            UserId = userId,
            GrantId = grantId,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            ChunkCount = chunks.Count,
            IsIndexed = false
        };
        await docRepo.AddAsync(document);

        // Embed and upsert with bounded parallelism (respect rate limits)
        var sem = new SemaphoreSlim(4);
        var tasks = chunks.Select(async chunk =>
        {
            await sem.WaitAsync();
            try
            {
                var vector = await embeddingService.EmbedAsync(chunk.Text);
                await docVectorService.UpsertDocumentChunkAsync(
                    document.DocumentId, chunk.ChunkIndex, chunk.TotalChunks,
                    vector, grantId, userId, fileName, chunk.Text);
            }
            finally { sem.Release(); }
        });

        await Task.WhenAll(tasks);
        await docRepo.UpdateChunkCountAsync(document.DocumentId, chunks.Count);

        logger.LogInformation("Indexed document {FileName} — {ChunkCount} chunks for grant {GrantId}",
            fileName, chunks.Count, grantId);

        return document;
    }

    public async Task DeleteAsync(Guid documentId)
    {
        var doc = await docRepo.GetByIdAsync(documentId);
        if (doc is null) return;

        await docVectorService.DeleteDocumentAsync(documentId);
        await docRepo.DeleteAsync(documentId);

        logger.LogInformation("Deleted document {DocumentId} ({FileName})", documentId, doc.FileName);
    }

    private static async Task<string> ExtractTextAsync(Stream stream, string ext)
        => ext switch
        {
            ".pdf"  => ExtractFromPdf(stream),
            ".docx" => ExtractFromDocx(stream),
            ".txt"  => await ExtractFromText(stream),
            _       => throw new InvalidOperationException($"Unsupported extension: {ext}")
        };

    private static string ExtractFromPdf(Stream stream)
    {
        using var doc = PdfDocument.Open(stream);
        var sb = new System.Text.StringBuilder();
        foreach (var page in doc.GetPages())
        {
            var words = page.GetWords();
            sb.AppendLine(string.Join(" ", words.Select(w => w.Text)));
        }
        return sb.ToString();
    }

    private static string ExtractFromDocx(Stream stream)
    {
        using var doc = WordprocessingDocument.Open(stream, false);
        return doc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
    }

    private static async Task<string> ExtractFromText(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
