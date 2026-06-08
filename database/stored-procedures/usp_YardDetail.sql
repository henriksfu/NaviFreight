CREATE OR ALTER PROCEDURE dbo.usp_YardDetail
    @TenantId NVARCHAR(50),
    @YardId   INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: yard header with aggregated dock counts
    SELECT
        y.YardId,
        y.YardName,
        y.Capacity,
        y.OccupiedSlots,
        y.InboundQueue,
        y.AverageTurnMinutes,
        COUNT(d.DockId)                                                        AS TotalDocks,
        SUM(CASE WHEN d.Status = 'Available' THEN 1 ELSE 0 END)               AS AvailableDocks,
        y.IsActive,
        y.CreatedUtc,
        y.UpdatedUtc
    FROM dbo.Yards y
    LEFT JOIN dbo.Docks d ON d.YardId = y.YardId
    WHERE y.TenantId = @TenantId
      AND y.YardId   = @YardId
    GROUP BY
        y.YardId, y.YardName, y.Capacity, y.OccupiedSlots,
        y.InboundQueue, y.AverageTurnMinutes, y.IsActive, y.CreatedUtc, y.UpdatedUtc;

    -- Result set 2: all docks for this yard
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
    WHERE d.YardId = @YardId
    ORDER BY d.DockCode;
END;
