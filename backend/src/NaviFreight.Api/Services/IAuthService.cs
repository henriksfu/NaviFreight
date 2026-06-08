using NaviFreight.Api.Models;

namespace NaviFreight.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string? tenantId = null);
    Task<UserProfile?> GetUserByEmailAsync(string email, string tenantId);
}
