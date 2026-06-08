CREATE OR ALTER PROCEDURE dbo.usp_YardOperationalUpdate
    @TenantId           NVARCHAR(50),
    @YardId             INT,
    @OccupiedSlots      INT,
    @InboundQueue       INT,
    @AverageTurnMinutes INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Yards
    SET OccupiedSlots      = @OccupiedSlots,
        InboundQueue       = @InboundQueue,
        AverageTurnMinutes = @AverageTurnMinutes,
        UpdatedUtc         = SYSUTCDATETIME()
    WHERE YardId   = @YardId
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
