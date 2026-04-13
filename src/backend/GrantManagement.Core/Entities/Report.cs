namespace GrantManagement.Core.Entities;

public class Report
{
    public Guid ReportId { get; set; } = Guid.NewGuid();
    public Guid GrantId { get; set; }
    public int ReportingYear { get; set; }
    public string ReportingQuarter { get; set; } = string.Empty; // Q1, Q2, Q3, Q4, Annual
    public string ReportType { get; set; } = string.Empty; // Progress, Final
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public int? ReviewerRating { get; set; } // 1-5
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    public Grant Grant { get; set; } = null!;
    public ICollection<ReportSection> Sections { get; set; } = [];
    public ICollection<AIUsageLog> AIUsageLogs { get; set; } = [];
}
