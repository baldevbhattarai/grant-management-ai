using GrantManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrantManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Get all active grantee users (for demo user selector)</summary>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await db.Users
            .Where(u => u.IsActive && u.Role == "Grantee")
            .OrderBy(u => u.FirstName)
            .Select(u => new UserDto(u.UserId, u.FirstName + " " + u.LastName, u.OrganizationName ?? string.Empty))
            .ToListAsync();

        return Ok(users);
    }
}

public record UserDto(Guid UserId, string FullName, string OrganizationName);
