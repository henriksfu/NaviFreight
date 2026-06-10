namespace NaviFreight.Api.Contracts;

public sealed record UserResponse(
    int UserId,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTime CreatedUtc);
