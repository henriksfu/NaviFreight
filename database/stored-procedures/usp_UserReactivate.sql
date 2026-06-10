CREATE OR ALTER PROCEDURE dbo.usp_UserReactivate
    @TenantId NVARCHAR(50),
    @UserId   INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET IsActive = 1, UpdatedUtc = GETUTCDATE()
    WHERE UserId = @UserId AND TenantId = @TenantId AND IsActive = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT NULL WHERE 1 = 0;
        RETURN;
    END;

    SELECT u.UserId, u.EmailAddress, u.DisplayName, ur.RoleName, u.IsActive, u.CreatedUtc
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON ur.UserRoleId = u.UserRoleId
    WHERE u.UserId = @UserId;
END;
GO
