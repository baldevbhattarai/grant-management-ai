namespace GrantManagement.Core.Entities;

public class Grant
{
    public Guid GrantId { get; set; } = Guid.NewGuid();
    public string GrantNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string GrantType { get; set; } = string.Empty; // C16, C17, C18, H80
    public string ProgramName { get; set; } = string.Empty;
    public int ProgramTypeCode { get; set; }
    public string? FocusAreas { get; set; } // JSON array
    public decimal? FundingAmount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Closed, Suspended
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<AIApprovedContent> ApprovedContent { get; set; } = [];
}
