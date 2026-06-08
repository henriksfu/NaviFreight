CREATE OR ALTER PROCEDURE dbo.usp_YardUpdate
    @TenantId NVARCHAR(50),
    @YardId   INT,
    @YardName NVARCHAR(150),
    @Capacity INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Yards
    SET YardName   = @YardName,
        Capacity   = @Capacity,
        UpdatedUtc = SYSUTCDATETIME()
    WHERE YardId  = @YardId
      AND TenantId = @TenantId
      AND IsActive = 1;

    IF @@ROWCOUNT = 0
        RETURN;

    SELECT
        y.YardId,
        y.YardName,
        y.Capacity,
        y.OccupiedSlots,
        y.InboundQueue,
        y.AverageTurnMinutes,
        COUNT(d.DockId)                                              AS TotalDocks,
        SUM(CASE WHEN d.Status = 'Available' THEN 1 ELSE 0 END)     AS AvailableDocks,
        y.IsActive,
        y.CreatedUtc,
        y.UpdatedUtc
    FROM dbo.Yards y
    LEFT JOIN dbo.Docks d ON d.YardId = y.YardId
    WHERE y.YardId = @YardId
    GROUP BY
        y.YardId, y.YardName, y.Capacity, y.OccupiedSlots,
        y.InboundQueue, y.AverageTurnMinutes, y.IsActive, y.CreatedUtc, y.UpdatedUtc;
END;
