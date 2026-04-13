namespace GrantManagement.Core.Entities;

public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Grantee, Reviewer, Admin
    public string? OrganizationName { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Grant> Grants { get; set; } = [];
    public ICollection<AIUsageLog> AIUsageLogs { get; set; } = [];
}
