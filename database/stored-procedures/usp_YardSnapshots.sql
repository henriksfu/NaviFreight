CREATE OR ALTER PROCEDURE dbo.usp_YardSnapshots
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        y.YardName,
        y.OccupiedSlots,
        y.Capacity AS TotalSlots,
        y.InboundQueue,
        SUM(CASE WHEN d.Status = 'Available' THEN 1 ELSE 0 END) AS AvailableDocks,
        y.AverageTurnMinutes
    FROM dbo.Yards y
    LEFT JOIN dbo.Docks d
        ON d.YardId = y.YardId
    WHERE y.TenantId = @TenantId
    GROUP BY
        y.YardName,
        y.OccupiedSlots,
        y.Capacity,
        y.InboundQueue,
        y.AverageTurnMinutes
    ORDER BY y.YardName;
END;
