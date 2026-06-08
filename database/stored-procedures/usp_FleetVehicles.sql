CREATE OR ALTER PROCEDURE dbo.usp_FleetVehicles
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.VehicleId,
        v.DriverName,
        v.Status,
        y.YardName AS CurrentYard,
        v.LastUpdatedUtc,
        v.EtaUtc,
        v.UtilizationPercent,
        r.RouteCode
    FROM dbo.Vehicles v
    LEFT JOIN dbo.Yards y
        ON y.YardId = v.CurrentYardId
    LEFT JOIN dbo.Routes r
        ON r.RouteId = v.RouteId
    WHERE v.TenantId = @TenantId
    ORDER BY v.LastUpdatedUtc DESC;
END;
