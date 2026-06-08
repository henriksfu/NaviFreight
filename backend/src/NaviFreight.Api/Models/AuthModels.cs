namespace NaviFreight.Api.Models;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    UserProfile User);

public sealed record UserProfile(
    int UserId,
    string Email,
    string DisplayName,
    string Role,
    string TenantId);

public sealed record UserIdentity(
    int UserId,
    string TenantId,
    string Email,
    string DisplayName,
    string Role,
    string PasswordHash);
