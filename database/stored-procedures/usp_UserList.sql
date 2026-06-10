CREATE OR ALTER PROCEDURE dbo.usp_UserList
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.UserId,
        u.EmailAddress,
        u.DisplayName,
        ur.RoleName,
        u.IsActive,
        u.CreatedUtc
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON ur.UserRoleId = u.UserRoleId
    WHERE u.TenantId = @TenantId
    ORDER BY u.DisplayName;
END;
GO
