CREATE OR ALTER PROCEDURE dbo.usp_DockAssignVehicle
    @TenantId  NVARCHAR(50),
    @YardId    INT,
    @DockId    INT,
    @VehicleId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Verify vehicle belongs to this tenant
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Vehicles
        WHERE VehicleId = @VehicleId AND TenantId = @TenantId
    )
    BEGIN
        SELECT CAST(NULL AS INT) AS DockId WHERE 1 = 0;
        RETURN;
    END;

    UPDATE dbo.Docks
    SET Status             = 'Occupied',
        OccupyingVehicleId = @VehicleId,
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
