CREATE OR ALTER PROCEDURE dbo.usp_DockDetail
    @TenantId NVARCHAR(50),
    @YardId   INT,
    @DockId   INT
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
    WHERE d.DockId    = @DockId
      AND d.YardId    = @YardId
      AND y.TenantId  = @TenantId;
END;
