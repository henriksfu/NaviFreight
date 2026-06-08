CREATE OR ALTER PROCEDURE dbo.usp_YardDelete
    @TenantId NVARCHAR(50),
    @YardId   INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Soft delete: preserve FK references from Routes and historical data
    UPDATE dbo.Yards
    SET IsActive   = 0,
        UpdatedUtc = SYSUTCDATETIME()
    WHERE YardId   = @YardId
      AND TenantId = @TenantId
      AND IsActive = 1;

    SELECT @@ROWCOUNT AS AffectedRows;
END;
