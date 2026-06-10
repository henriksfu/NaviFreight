CREATE OR ALTER PROCEDURE dbo.usp_UserUpdate
    @TenantId       NVARCHAR(50),
    @UserId         INT,
    @DisplayName    NVARCHAR(150),
    @RoleName       NVARCHAR(50),
    @NewPasswordHash NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserRoleId INT;
    SELECT @UserRoleId = UserRoleId FROM dbo.UserRoles WHERE RoleName = @RoleName;

    IF @UserRoleId IS NULL
    BEGIN
        RAISERROR('Invalid role name: %s', 16, 1, @RoleName);
        RETURN;
    END;

    UPDATE dbo.Users
    SET
        DisplayName  = @DisplayName,
        UserRoleId   = @UserRoleId,
        PasswordHash = CASE WHEN @NewPasswordHash IS NOT NULL THEN @NewPasswordHash ELSE PasswordHash END,
        UpdatedUtc   = GETUTCDATE()
    WHERE UserId = @UserId AND TenantId = @TenantId;

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
