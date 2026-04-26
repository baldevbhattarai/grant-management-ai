namespace GrantManagement.Core.DTOs;

public class SuggestionRequestDto
{
    public Guid ReportId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    /// <summary>Optional key highlights for the current reporting period provided by the user.</summary>
    public string? KeyPoints { get; set; }
    /// <summary>Refinement instruction for regeneration, e.g. "make it shorter" or "focus more on outcomes".</summary>
    public string? RegenerationFeedback { get; set; }
}

public class SuggestionResponseDto
{
    public bool Success { get; set; }
    public string? SuggestedText { get; set; }
    public string? ErrorMessage { get; set; }
    public int TokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }
    /// <summary>Log ID returned so the frontend can send feedback referencing this generation.</summary>
    public Guid? LogId { get; set; }
}

public class FeedbackRequestDto
{
    public Guid LogId { get; set; }
    public string UserAction { get; set; } = string.Empty; // Accepted, Rejected, Edited, Regenerated
    public int? UserRating { get; set; }
    /// <summary>The accepted/edited text — provided so accepted suggestions can be promoted to the example pool.</summary>
    public string? AcceptedText { get; set; }
}
