namespace GrantManagement.Core.DTOs;

public class ReportDto
{
    public Guid ReportId { get; set; }
    public Guid GrantId { get; set; }
    public string GrantNumber { get; set; } = string.Empty;
    public int ReportingYear { get; set; }
    public string ReportingQuarter { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public int? ReviewerRating { get; set; }
    public List<ReportSectionDto> Sections { get; set; } = [];
}

public class ReportSectionDto
{
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public int SectionOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string? ResponseText { get; set; }
    public decimal? ResponseNumber { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxLength { get; set; }
}

public class UpdateSectionRequest
{
    public string? ResponseText { get; set; }
    public decimal? ResponseNumber { get; set; }
    public string? ResponseSingle { get; set; }
    public string? ResponseOptions { get; set; }
}
