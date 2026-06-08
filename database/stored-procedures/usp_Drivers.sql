CREATE OR ALTER PROCEDURE dbo.usp_Drivers
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.DriverId,
        d.FullName,
        d.LicenseNumber,
        d.AvailabilityStatus,
        v.VehicleId AS AssignedVehicleId
    FROM dbo.Drivers d
    LEFT JOIN dbo.Vehicles v
        ON v.DriverId = d.DriverId
       AND v.TenantId = d.TenantId
    WHERE d.TenantId = @TenantId
    ORDER BY d.FullName;
END;
