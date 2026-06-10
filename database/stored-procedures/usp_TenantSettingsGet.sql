CREATE OR ALTER PROCEDURE dbo.usp_TenantSettingsGet
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT SettingKey AS [Key], SettingValue AS Value
    FROM dbo.TenantSettings
    WHERE TenantId = @TenantId
    ORDER BY SettingKey;
END;
GO
