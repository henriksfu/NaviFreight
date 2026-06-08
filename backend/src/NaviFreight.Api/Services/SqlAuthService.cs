using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NaviFreight.Api.Configuration;
using NaviFreight.Api.Models;
using NaviFreight.Api.Repositories;

namespace NaviFreight.Api.Services;

public sealed class SqlAuthService(IAuthRepository authRepository, IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string? tenantId = null)
    {
        var user = await authRepository.GetUserByEmailAsync(request.Email, tenantId);

        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var expiry = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);
        var token = GenerateToken(user, expiry);

        return new LoginResponse(
            token,
            expiry,
            new UserProfile(user.UserId, user.Email, user.DisplayName, user.Role, user.TenantId));
    }

    public async Task<UserProfile?> GetUserByEmailAsync(string email, string tenantId)
    {
        var user = await authRepository.GetUserByEmailAsync(email, tenantId);

        return user is null
            ? null
            : new UserProfile(user.UserId, user.Email, user.DisplayName, user.Role, user.TenantId);
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

    private static bool VerifyPassword(string password, string storedHash)
    {
        var candidateHash = HashPassword(password);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(candidateHash),
            Encoding.UTF8.GetBytes(storedHash));
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
