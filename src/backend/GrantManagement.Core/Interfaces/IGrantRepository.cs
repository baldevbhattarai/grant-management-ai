using GrantManagement.Core.Entities;

namespace GrantManagement.Core.Interfaces;

public interface IGrantRepository
{
    Task<List<Grant>> GetByUserIdAsync(Guid userId);
    Task<Grant?> GetByIdAsync(Guid grantId);
    Task<Grant?> GetByIdWithReportsAsync(Guid grantId);
}
