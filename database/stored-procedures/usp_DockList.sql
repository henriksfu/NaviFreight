CREATE OR ALTER PROCEDURE dbo.usp_DockList
    @TenantId NVARCHAR(50),
    @YardId   INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.DockId,
        d.YardId,
        d.DockCode,
        d.Status,
        d.OccupyingVehicleId,
        d.Notes,
        d.CreatedUtc,
        d.UpdatedUtc
    FROM dbo.Docks d
    INNER JOIN dbo.Yards y ON y.YardId = d.YardId
    WHERE d.YardId    = @YardId
      AND y.TenantId  = @TenantId
      AND y.IsActive  = 1
    ORDER BY d.DockCode;
END;
