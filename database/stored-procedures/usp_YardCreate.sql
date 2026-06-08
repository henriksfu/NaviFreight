CREATE OR ALTER PROCEDURE dbo.usp_YardCreate
    @TenantId  NVARCHAR(50),
    @YardName  NVARCHAR(150),
    @Capacity  INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Yards (TenantId, YardName, Capacity, OccupiedSlots, InboundQueue, AverageTurnMinutes, IsActive)
    VALUES (@TenantId, @YardName, @Capacity, 0, 0, 0, 1);

    DECLARE @NewYardId INT = SCOPE_IDENTITY();

    SELECT
        y.YardId,
        y.YardName,
        y.Capacity,
        y.OccupiedSlots,
        y.InboundQueue,
        y.AverageTurnMinutes,
        0     AS TotalDocks,
        0     AS AvailableDocks,
        y.IsActive,
        y.CreatedUtc,
        y.UpdatedUtc
    FROM dbo.Yards y
    WHERE y.YardId = @NewYardId;
END;
