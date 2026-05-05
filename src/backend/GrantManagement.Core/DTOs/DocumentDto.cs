namespace GrantManagement.Core.DTOs;

public record UploadedDocumentDto(
    Guid DocumentId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    int ChunkCount,
    DateTime UploadedAt,
    bool IsIndexed);
