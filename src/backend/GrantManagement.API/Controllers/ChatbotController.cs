using GrantManagement.Core.DTOs;
using GrantManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GrantManagement.API.Controllers;

[ApiController]
[Route("api/ai/chat")]
public class ChatbotController(IChatbotService chatbotService) : ControllerBase
{
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
