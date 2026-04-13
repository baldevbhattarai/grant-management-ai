namespace GrantManagement.Core.Entities;

public class ReportSection
{
    public Guid SectionId { get; set; } = Guid.NewGuid();
    public Guid ReportId { get; set; }
    public string SectionName { get; set; } = string.Empty; // PerformanceNarrative, Accomplishments, etc.
    public string SectionTitle { get; set; } = string.Empty;
    public int SectionOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty; // Text, Number, MultiSelect, Radio
    public string? ResponseText { get; set; }
    public decimal? ResponseNumber { get; set; }
    public string? ResponseOptions { get; set; } // JSON array for multi-select
    public string? ResponseSingle { get; set; } // For radio/single-select
    public bool IsRequired { get; set; } = true;
    public int? MaxLength { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    public Report Report { get; set; } = null!;
}
