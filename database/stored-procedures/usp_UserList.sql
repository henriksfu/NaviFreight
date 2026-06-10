CREATE OR ALTER PROCEDURE dbo.usp_UserList
    @TenantId NVARCHAR(50),
    @Page     INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT
        COUNT(*) OVER()  AS TotalCount,
        u.UserId,
        u.EmailAddress,
        u.DisplayName,
        ur.RoleName,
        u.IsActive,
        u.CreatedUtc
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON ur.UserRoleId = u.UserRoleId
    WHERE u.TenantId = @TenantId
    ORDER BY u.DisplayName
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO
