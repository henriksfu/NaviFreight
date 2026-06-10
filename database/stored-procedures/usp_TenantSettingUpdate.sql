CREATE OR ALTER PROCEDURE dbo.usp_TenantSettingUpdate
    @TenantId    NVARCHAR(50),
    @SettingKey  NVARCHAR(100),
    @SettingValue NVARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.TenantSettings WHERE TenantId = @TenantId AND SettingKey = @SettingKey)
    BEGIN
        UPDATE dbo.TenantSettings
        SET SettingValue = @SettingValue
        WHERE TenantId = @TenantId AND SettingKey = @SettingKey;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.TenantSettings (TenantId, SettingKey, SettingValue)
        VALUES (@TenantId, @SettingKey, @SettingValue);
    END

    SELECT SettingKey AS [Key], SettingValue AS Value
    FROM dbo.TenantSettings
    WHERE TenantId = @TenantId AND SettingKey = @SettingKey;
END;
GO
