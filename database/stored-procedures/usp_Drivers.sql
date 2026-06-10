CREATE OR ALTER PROCEDURE dbo.usp_Drivers
    @TenantId NVARCHAR(50),
    @Page     INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT
        COUNT(*) OVER()              AS TotalCount,
        d.DriverId,
        d.FullName,
        d.LicenseNumber,
        d.AvailabilityStatus,
        v.VehicleId                  AS AssignedVehicleId
    FROM dbo.Drivers d
    LEFT JOIN dbo.Vehicles v
        ON  v.DriverId  = d.DriverId
        AND v.TenantId  = d.TenantId
    WHERE d.TenantId = @TenantId
    ORDER BY d.FullName
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
