using GrantManagement.Core.Entities;

namespace GrantManagement.Core.Interfaces;

public interface IChatRepository
{
    /// <summary>Persists one user question and one assistant answer as a conversation turn.</summary>
    Task SaveTurnAsync(Guid sessionId, Guid userId, Guid grantId, string question, string answer);

    /// <summary>Returns the last <paramref name="maxTurns"/> turns (user+assistant pairs) in chronological order.</summary>
    Task<List<ChatConversation>> GetHistoryAsync(Guid sessionId, int maxTurns = 5);
}
