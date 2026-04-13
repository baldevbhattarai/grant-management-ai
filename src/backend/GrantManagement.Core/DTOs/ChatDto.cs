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
}

public class ChatSourceDto
{
    public string ReportPeriod { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
}
