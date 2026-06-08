CREATE OR ALTER PROCEDURE dbo.usp_FleetVehicleUnassignDriver
    @TenantId NVARCHAR(50),
    @VehicleId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Vehicles
    SET DriverId = NULL,
        DriverName = 'Unassigned',
        LastUpdatedUtc = SYSUTCDATETIME()
    WHERE TenantId = @TenantId
      AND VehicleId = @VehicleId;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 52002, 'Vehicle not found for tenant.', 1;
    END;
END;
