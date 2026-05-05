namespace GrantManagement.Core.Entities;

public class AIUploadedDocument
{
    public Guid DocumentId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid GrantId { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public int ChunkCount { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsIndexed { get; set; }

    public Grant? Grant { get; set; }
}
