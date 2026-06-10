using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NaviFreight.Api.Configuration;
using NaviFreight.Api.Models;

namespace NaviFreight.Api.Services;

public sealed class InMemoryAuthService : IAuthService
{
    private readonly JwtOptions _jwt;
    private readonly IReadOnlyList<UserIdentity> _users;

    public InMemoryAuthService(IOptions<JwtOptions> jwtOptions)
    {
        _jwt = jwtOptions.Value;
        _users = BuildSeedUsers();
    }

    public Task<LoginResponse?> LoginAsync(LoginRequest request, string? tenantId = null)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) &&
            (tenantId is null || u.TenantId == tenantId));

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Task.FromResult<LoginResponse?>(null);

        var expiry = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);
        var token = GenerateToken(user, expiry);

        var response = new LoginResponse(
            token,
            expiry,
            new UserProfile(user.UserId, user.Email, user.DisplayName, user.Role, user.TenantId));

        return Task.FromResult<LoginResponse?>(response);
    }

    public Task<UserProfile?> GetUserByEmailAsync(string email, string tenantId)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
            u.TenantId == tenantId);

        UserProfile? profile = user is null
            ? null
            : new UserProfile(user.UserId, user.Email, user.DisplayName, user.Role, user.TenantId);

        return Task.FromResult(profile);
    }

    private string GenerateToken(UserIdentity user, DateTime expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim("role", user.Role),
            new Claim("tenant_id", user.TenantId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Pre-computed bcrypt hashes (work factor 12) for demo passwords.
    // Regenerate: BCrypt.Net.BCrypt.HashPassword("<password>", 12)
    private static IReadOnlyList<UserIdentity> BuildSeedUsers() =>
    [
        new(1, "tenant-demo",
            "morgan.ellis@atlasmeridian.example", "Morgan Ellis",
            "Tenant Admin",
            "$2a$12$MFLfkBBc6cuy86Kn8CMFq.niD5ajgkItCIUs/9EZ1jvMZq2Fd0hVS"),

        new(2, "tenant-demo",
            "priya.shah@atlasmeridian.example", "Priya Shah",
            "Dispatcher",
            "$2a$12$ZWlwwEtOnQuTjNRlq8npI.JoU5gWi9c8c4hIUYM.PB6RtqXwBsEDW"),

        new(3, "tenant-demo",
            "darius.cole@atlasmeridian.example", "Darius Cole",
            "Yard Manager",
            "$2a$12$REhXC21XJV1pbM7oVDv40OdWSWEEt.ItL8yGoyA7bwaMRZ46zkj8O")
    ];
}
