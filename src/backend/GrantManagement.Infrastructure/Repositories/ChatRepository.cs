using GrantManagement.Core.DTOs;
using GrantManagement.Core.Entities;
using GrantManagement.Core.Interfaces;
using GrantManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GrantManagement.Infrastructure.Repositories;

public class ChatRepository(ApplicationDbContext context) : IChatRepository
{
    public async Task SaveTurnAsync(Guid sessionId, Guid userId, Guid grantId, string question, string answer)
    {
        context.ChatConversations.AddRange(
            new ChatConversation
            {
                SessionId = sessionId,
                UserId = userId,
                GrantId = grantId,
                Role = "user",
                Content = question
            },
            new ChatConversation
            {
                SessionId = sessionId,
                UserId = userId,
                GrantId = grantId,
                Role = "assistant",
                Content = answer
            }
        );
        await context.SaveChangesAsync();
    }

    public async Task<List<ChatConversation>> GetHistoryAsync(Guid sessionId, int maxTurns = 5)
    {
        // Load last maxTurns * 2 messages (each turn = 1 user + 1 assistant), including any summary row
        var messages = await context.ChatConversations
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.CreatedDate)
            .Take(maxTurns * 2)
            .ToListAsync();

        messages.Reverse(); // restore chronological order
        return messages;
    }

    public async Task<int> GetTurnCountAsync(Guid sessionId)
    {
        var total = await context.ChatConversations
            .CountAsync(c => c.SessionId == sessionId && (c.Role == "user" || c.Role == "assistant"));
        return total / 2; // each turn = 1 user + 1 assistant message
    }

    public async Task ReplaceTurnsWithSummaryAsync(Guid sessionId, Guid userId, Guid grantId, string summary)
    {
        var existing = await context.ChatConversations
            .Where(c => c.SessionId == sessionId)
            .ToListAsync();

        context.ChatConversations.RemoveRange(existing);

        context.ChatConversations.Add(new ChatConversation
        {
            SessionId = sessionId,
            UserId = userId,
            GrantId = grantId,
            Role = "summary",
            Content = summary
        });

        await context.SaveChangesAsync();
    }

    public async Task<List<ChatSessionSummary>> GetSessionsAsync(Guid userId, Guid grantId, int maxSessions = 20)
    {
        // Group by SessionId, get first user message and last message per session
        var sessions = await context.ChatConversations
            .Where(c => c.UserId == userId && c.GrantId == grantId)
            .GroupBy(c => c.SessionId)
            .Select(g => new
            {
                SessionId = g.Key,
                MessageCount = g.Count(),
                StartedAt = g.Min(c => c.CreatedDate),
                LastActivityAt = g.Max(c => c.CreatedDate),
                // First user question for display
                FirstQuestion = g.Where(c => c.Role == "user").OrderBy(c => c.CreatedDate)
                    .Select(c => c.Content).FirstOrDefault() ?? string.Empty,
                // Summary row content if it exists
                Summary = g.Where(c => c.Role == "summary").Select(c => c.Content).FirstOrDefault()
            })
            .OrderByDescending(s => s.LastActivityAt)
            .Take(maxSessions)
            .ToListAsync();

        return sessions.Select(s => new ChatSessionSummary(
            s.SessionId,
            s.FirstQuestion.Length > 120 ? s.FirstQuestion[..120] + "…" : s.FirstQuestion,
            s.Summary,
            s.MessageCount,
            s.StartedAt,
            s.LastActivityAt)).ToList();
    }
}
