CREATE OR ALTER PROCEDURE dbo.usp_DockReleaseVehicle
    @TenantId NVARCHAR(50),
    @YardId   INT,
    @DockId   INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Docks
    SET Status             = 'Available',
        OccupyingVehicleId = NULL,
        UpdatedUtc         = SYSUTCDATETIME()
    FROM dbo.Docks d
    INNER JOIN dbo.Yards y ON y.YardId = d.YardId
    WHERE d.DockId   = @DockId
      AND d.YardId   = @YardId
      AND y.TenantId = @TenantId;

    IF @@ROWCOUNT = 0
        RETURN;

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
    WHERE d.DockId = @DockId;
END;
