CREATE OR ALTER PROCEDURE dbo.usp_UserDeactivate
    @TenantId NVARCHAR(50),
    @UserId   INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET IsActive = 0, UpdatedUtc = GETUTCDATE()
    WHERE UserId = @UserId AND TenantId = @TenantId AND IsActive = 1;
END;
GO
