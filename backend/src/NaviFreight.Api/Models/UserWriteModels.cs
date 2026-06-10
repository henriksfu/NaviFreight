namespace NaviFreight.Api.Models;

public sealed record CreateUserRequest(
    string Email,
    string DisplayName,
    string Role,
    string Password);

public sealed record UpdateUserRequest(
    string DisplayName,
    string Role,
    string? NewPassword);
