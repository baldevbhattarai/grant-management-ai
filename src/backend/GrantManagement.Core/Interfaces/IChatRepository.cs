using GrantManagement.Core.Entities;

namespace GrantManagement.Core.Interfaces;

public interface IChatRepository
{
    /// <summary>Persists one user question and one assistant answer as a conversation turn.</summary>
    Task SaveTurnAsync(Guid sessionId, Guid userId, Guid grantId, string question, string answer);

    /// <summary>Returns the last <paramref name="maxTurns"/> turns (user+assistant pairs) in chronological order.</summary>
    Task<List<ChatConversation>> GetHistoryAsync(Guid sessionId, int maxTurns = 5);

    /// <summary>Returns the total number of user+assistant message pairs in the session.</summary>
    Task<int> GetTurnCountAsync(Guid sessionId);

    /// <summary>Replaces all existing messages for a session with a single summary row.</summary>
    Task ReplaceTurnsWithSummaryAsync(Guid sessionId, Guid userId, Guid grantId, string summary);
}
