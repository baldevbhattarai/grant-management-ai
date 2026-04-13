namespace GrantManagement.Core.Entities;

public class AIUsageLog
{
    public Guid LogId { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public Guid GrantId { get; set; }
    public Guid? ReportId { get; set; }
    public string FeatureType { get; set; } = string.Empty; // ContentSuggestion, QA_Chatbot
    public string? SectionName { get; set; }
    public string? Question { get; set; } // For chatbot
    public string ModelName { get; set; } = string.Empty;
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int? ResponseTimeMs { get; set; }
    public string? UserAction { get; set; } // Accepted, Rejected, Edited, Regenerated
    public int? UserRating { get; set; } // 1-5
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
