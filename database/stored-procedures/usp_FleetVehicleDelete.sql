CREATE OR ALTER PROCEDURE dbo.usp_FleetVehicleDelete
    @TenantId NVARCHAR(50),
    @VehicleId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Vehicles
    WHERE TenantId = @TenantId
      AND VehicleId = @VehicleId;
END;
