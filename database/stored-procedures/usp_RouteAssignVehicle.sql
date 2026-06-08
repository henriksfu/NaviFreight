CREATE OR ALTER PROCEDURE dbo.usp_RouteAssignVehicle
    @TenantId NVARCHAR(50),
    @RouteCode NVARCHAR(50),
    @VehicleId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RouteId INT = (
        SELECT RouteId
        FROM dbo.Routes
        WHERE TenantId = @TenantId
          AND RouteCode = @RouteCode
    );

    IF @RouteId IS NULL
    BEGIN
        THROW 51001, 'Route not found for tenant.', 1;
    END;

    UPDATE dbo.Vehicles
    SET RouteId = @RouteId
    WHERE TenantId = @TenantId
      AND VehicleId = @VehicleId;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 51002, 'Vehicle not found for tenant.', 1;
    END;
END;
