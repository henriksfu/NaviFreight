CREATE OR ALTER PROCEDURE dbo.usp_UserCreate
    @TenantId    NVARCHAR(50),
    @Email       NVARCHAR(200),
    @DisplayName NVARCHAR(150),
    @RoleName    NVARCHAR(50),
    @PasswordHash NVARCHAR(200)
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

    INSERT INTO dbo.Users (TenantId, EmailAddress, DisplayName, UserRoleId, PasswordHash, IsActive, CreatedUtc, UpdatedUtc)
    VALUES (@TenantId, @Email, @DisplayName, @UserRoleId, @PasswordHash, 1, GETUTCDATE(), GETUTCDATE());

    DECLARE @NewId INT = SCOPE_IDENTITY();

    SELECT u.UserId, u.EmailAddress, u.DisplayName, ur.RoleName, u.IsActive, u.CreatedUtc
    FROM dbo.Users u
    INNER JOIN dbo.UserRoles ur ON ur.UserRoleId = u.UserRoleId
    WHERE u.UserId = @NewId;
END;
GO
