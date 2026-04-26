namespace GrantManagement.Core.DTOs;

public class ChatRequestDto
{
    public Guid? UserId { get; set; }
    public Guid GrantId { get; set; }
    public string Question { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; } // null = new conversation
}

public class ChatResponseDto
{
    public bool Success { get; set; }
    public string? Answer { get; set; }
    public Guid ConversationId { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ChatSourceDto> Sources { get; set; } = [];
    /// <summary>Highest cosine similarity score from vector search (0–1). Null when only keyword search was used.</summary>
    public float? ConfidenceScore { get; set; }
    /// <summary>2–3 suggested follow-up questions generated after the answer.</summary>
    public List<string> FollowUpQuestions { get; set; } = [];
}

public class ChatSourceDto
{
    public string ReportPeriod { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    /// <summary>Report ID for deep-linking to the source section in the UI.</summary>
    public Guid? ReportId { get; set; }
}
