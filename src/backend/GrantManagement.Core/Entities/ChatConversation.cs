namespace GrantManagement.Core.Entities;

public class ChatConversation
{
    public Guid ConversationId { get; set; } = Guid.NewGuid(); // per-message PK
    public Guid SessionId { get; set; }                        // groups messages into one conversation
    public Guid UserId { get; set; }
    public Guid GrantId { get; set; }
    public string Role { get; set; } = string.Empty;           // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
