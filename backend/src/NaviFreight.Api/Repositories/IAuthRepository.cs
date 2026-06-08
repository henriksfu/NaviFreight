using NaviFreight.Api.Models;

namespace NaviFreight.Api.Repositories;

public interface IAuthRepository
{
    Task<UserIdentity?> GetUserByEmailAsync(string email, string? tenantId, CancellationToken cancellationToken = default);
}
