namespace GrantManagement.Core.DTOs;

public class SuggestionRequestDto
{
    public Guid ReportId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    /// <summary>Optional key highlights for the current reporting period provided by the user.</summary>
    public string? KeyPoints { get; set; }
}

public class SuggestionResponseDto
{
    public bool Success { get; set; }
    public string? SuggestedText { get; set; }
    public string? ErrorMessage { get; set; }
    public int TokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }
}

public class FeedbackRequestDto
{
    public Guid LogId { get; set; }
    public string UserAction { get; set; } = string.Empty; // Accepted, Rejected, Edited, Regenerated
    public int? UserRating { get; set; }
}
