CREATE OR ALTER PROCEDURE dbo.usp_AlertClose
    @TenantId     NVARCHAR(50),
    @AlertEventId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AlertEvents
    SET Status     = 'Closed',
        IsActive   = 0,
        UpdatedUtc = SYSUTCDATETIME()
    WHERE AlertEventId = @AlertEventId
      AND TenantId     = @TenantId;

    SELECT @@ROWCOUNT AS AffectedRows;
END;
