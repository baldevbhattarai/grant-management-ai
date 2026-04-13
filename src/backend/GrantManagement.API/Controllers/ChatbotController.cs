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
}
