using GrantManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GrantManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController(IAIRepository aiRepo) : ControllerBase
{
    [HttpGet("usage")]
    public async Task<IActionResult> GetUsageSummary([FromQuery] int days = 30)
    {
        if (days is < 1 or > 365) return BadRequest("days must be between 1 and 365");
        var summary = await aiRepo.GetUsageSummaryAsync(days);
        return Ok(summary);
    }
}
