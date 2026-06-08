CREATE OR ALTER PROCEDURE dbo.usp_SettingsSections
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        'Tenant Controls' AS SectionTitle,
        CONCAT(SettingKey, ': ', SettingValue) AS ItemValue
    FROM dbo.TenantSettings
    WHERE TenantId = @TenantId

    UNION ALL

    SELECT
        'Dispatch Rules' AS SectionTitle,
        CONCAT(RuleName, ': ', RuleValue)
    FROM dbo.DispatchRules
    WHERE TenantId = @TenantId
      AND IsEnabled = 1;
END;
