using GrantManagement.Core.DTOs;
using GrantManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GrantManagement.API.Controllers;

[ApiController]
[Route("api/ai/chat")]
public class ChatbotController(IChatbotService chatbotService, IChatRepository chatRepo) : ControllerBase
{
    /// <summary>Returns past chat sessions for a user and grant, most recent first</summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions([FromQuery] Guid userId, [FromQuery] Guid grantId)
    {
        if (userId == Guid.Empty || grantId == Guid.Empty)
            return BadRequest("userId and grantId are required");
        var sessions = await chatRepo.GetSessionsAsync(userId, grantId);
        return Ok(sessions);
    }

    /// <summary>Returns the full message history for a specific session</summary>
    [HttpGet("sessions/{sessionId:guid}/history")]
    public async Task<IActionResult> GetSessionHistory(Guid sessionId)
    {
        var messages = await chatRepo.GetHistoryAsync(sessionId, maxTurns: 50);
        return Ok(messages.Select(m => new { m.Role, m.Content, m.CreatedDate }));
    }

    /// <summary>Ask a natural language question about grant data</summary>
    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Ask([FromBody] ChatRequestDto request)
    {
        if (request.GrantId == Guid.Empty || string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("GrantId and Question are required");

        var result = await chatbotService.AskAsync(request);

        return Ok(result);
    }

    /// <summary>Streaming version — returns tokens as Server-Sent Events</summary>
    [HttpPost("stream")]
    public async Task Stream([FromBody] ChatRequestDto request)
    {
        if (request.GrantId == Guid.Empty || string.IsNullOrWhiteSpace(request.Question))
        {
            Response.StatusCode = 400;
            return;
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var token in chatbotService.AskStreamAsync(request))
        {
            var escaped = token.Replace("\n", "\\n");
            await Response.WriteAsync($"data: {escaped}\n\n");
            await Response.Body.FlushAsync();
        }

        await Response.WriteAsync("data: [DONE]\n\n");
        await Response.Body.FlushAsync();
    }
}
