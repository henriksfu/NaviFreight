CREATE OR ALTER PROCEDURE dbo.usp_YardList
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

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
      AND y.IsActive  = 1
    GROUP BY
        y.YardId, y.YardName, y.Capacity, y.OccupiedSlots,
        y.InboundQueue, y.AverageTurnMinutes, y.IsActive, y.CreatedUtc, y.UpdatedUtc
    ORDER BY y.YardName;
END;
