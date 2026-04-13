namespace GrantManagement.Core.Entities;

public class AIApprovedContent
{
    public Guid ContentId { get; set; } = Guid.NewGuid();
    public Guid GrantId { get; set; }
    public Guid? ReportId { get; set; }
    public int ProgramTypeCode { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
    public int? ReviewerRating { get; set; } // 1-5 (4-5 = high quality)
    public string? GrantType { get; set; }
    public string? Keywords { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Grant Grant { get; set; } = null!;
}
