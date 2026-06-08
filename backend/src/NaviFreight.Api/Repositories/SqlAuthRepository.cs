using System.Data;
using Microsoft.Data.SqlClient;
using NaviFreight.Api.Data;
using NaviFreight.Api.Models;

namespace NaviFreight.Api.Repositories;

public sealed class SqlAuthRepository(ISqlConnectionFactory connectionFactory) : IAuthRepository
{
    public async Task<UserIdentity?> GetUserByEmailAsync(
        string email,
        string? tenantId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(@"
SELECT TOP (1)
    u.UserId,
    u.TenantId,
    u.EmailAddress,
    u.DisplayName,
    ur.RoleName,
    u.PasswordHash
FROM dbo.Users u
INNER JOIN dbo.UserRoles ur
    ON ur.UserRoleId = u.UserRoleId
WHERE u.EmailAddress = @Email
  AND u.IsActive = 1
  AND (@TenantId IS NULL OR u.TenantId = @TenantId);", connection);

        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200) { Value = email });
        command.Parameters.Add(new SqlParameter("@TenantId", SqlDbType.NVarChar, 50)
        {
            Value = tenantId is null ? DBNull.Value : tenantId
        });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        // A user without a password hash cannot authenticate.
        if (reader.IsDBNull(5))
            return null;

        return new UserIdentity(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5));
    }
}
