namespace GrantManagement.Core.DTOs;

public class GrantDto
{
    public Guid GrantId { get; set; }
    public string GrantNumber { get; set; } = string.Empty;
    public string GrantType { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public int ProgramTypeCode { get; set; }
    public string? FocusAreas { get; set; }
    public decimal? FundingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
}
