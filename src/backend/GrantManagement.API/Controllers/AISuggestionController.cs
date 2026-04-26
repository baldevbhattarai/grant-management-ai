using GrantManagement.Core.DTOs;
using GrantManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GrantManagement.API.Controllers;

[ApiController]
[Route("api/ai/suggestions")]
public class AISuggestionController(IContentSuggestionService suggestionService) : ControllerBase
{
    /// <summary>Generate an AI content suggestion for a report section</summary>
    [HttpPost]
    public async Task<ActionResult<SuggestionResponseDto>> Generate([FromBody] SuggestionRequestDto request)
    {
        if (request.ReportId == Guid.Empty || string.IsNullOrWhiteSpace(request.SectionName))
            return BadRequest("ReportId and SectionName are required");

        var result = await suggestionService.GenerateSuggestionAsync(request);

        return Ok(result);
    }

    /// <summary>Streaming version — returns tokens as Server-Sent Events</summary>
    [HttpPost("stream")]
    public async Task Stream([FromBody] SuggestionRequestDto request)
    {
        if (request.ReportId == Guid.Empty || string.IsNullOrWhiteSpace(request.SectionName))
        {
            Response.StatusCode = 400;
            return;
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var token in suggestionService.StreamSuggestionAsync(request))
        {
            var escaped = token.Replace("\n", "\\n");
            await Response.WriteAsync($"data: {escaped}\n\n");
            await Response.Body.FlushAsync();
        }

        await Response.WriteAsync("data: [DONE]\n\n");
        await Response.Body.FlushAsync();
    }

    /// <summary>Record user feedback (accepted/rejected/edited) on a suggestion</summary>
    [HttpPost("feedback")]
    public async Task<IActionResult> Feedback([FromBody] FeedbackRequestDto request)
    {
        await suggestionService.RecordFeedbackAsync(request);
        return NoContent();
    }
}
