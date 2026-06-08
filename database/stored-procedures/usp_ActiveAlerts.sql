CREATE OR ALTER PROCEDURE dbo.usp_ActiveAlerts
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Severity,
        Title,
        Description,
        OwnerTeam AS Owner
    FROM dbo.AlertEvents
    WHERE TenantId = @TenantId
      AND IsActive = 1
    ORDER BY
        CASE Severity
            WHEN 'Critical' THEN 1
            WHEN 'Warning' THEN 2
            ELSE 3
        END,
        CreatedUtc DESC;
END;
