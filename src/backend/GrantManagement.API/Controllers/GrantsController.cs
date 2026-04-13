using GrantManagement.Core.DTOs;
using GrantManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GrantManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GrantsController(IGrantRepository grantRepo) : ControllerBase
{
    /// <summary>Get all grants for a user</summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<GrantDto>>> GetByUser(Guid userId)
    {
        var grants = await grantRepo.GetByUserIdAsync(userId);
        var dtos = grants.Select(g => new GrantDto
        {
            GrantId = g.GrantId,
            GrantNumber = g.GrantNumber,
            GrantType = g.GrantType,
            ProgramName = g.ProgramName,
            ProgramTypeCode = g.ProgramTypeCode,
            FocusAreas = g.FocusAreas,
            FundingAmount = g.FundingAmount,
            Status = g.Status,
            UserName = $"{g.User.FirstName} {g.User.LastName}",
            OrganizationName = g.User.OrganizationName ?? string.Empty
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>Get a single grant with its reports</summary>
    [HttpGet("{grantId:guid}")]
    public async Task<ActionResult<GrantDto>> GetById(Guid grantId)
    {
        var grant = await grantRepo.GetByIdAsync(grantId);
        if (grant is null) return NotFound();

        return Ok(new GrantDto
        {
            GrantId = grant.GrantId,
            GrantNumber = grant.GrantNumber,
            GrantType = grant.GrantType,
            ProgramName = grant.ProgramName,
            ProgramTypeCode = grant.ProgramTypeCode,
            FocusAreas = grant.FocusAreas,
            FundingAmount = grant.FundingAmount,
            Status = grant.Status,
            UserName = $"{grant.User.FirstName} {grant.User.LastName}",
            OrganizationName = grant.User.OrganizationName ?? string.Empty
        });
    }
}
