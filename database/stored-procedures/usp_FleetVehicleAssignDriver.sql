CREATE OR ALTER PROCEDURE dbo.usp_FleetVehicleAssignDriver
    @TenantId NVARCHAR(50),
    @VehicleId NVARCHAR(50),
    @DriverId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DriverName NVARCHAR(150) = (
        SELECT FullName
        FROM dbo.Drivers
        WHERE TenantId = @TenantId
          AND DriverId = @DriverId
    );

    IF @DriverName IS NULL
    BEGIN
        THROW 52001, 'Driver not found for tenant.', 1;
    END;

    UPDATE dbo.Vehicles
    SET DriverId = @DriverId,
        DriverName = @DriverName,
        LastUpdatedUtc = SYSUTCDATETIME()
    WHERE TenantId = @TenantId
      AND VehicleId = @VehicleId;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 52002, 'Vehicle not found for tenant.', 1;
    END;
END;
