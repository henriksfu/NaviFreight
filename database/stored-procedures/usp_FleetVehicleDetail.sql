CREATE OR ALTER PROCEDURE dbo.usp_FleetVehicleDetail
    @TenantId NVARCHAR(50),
    @VehicleId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.VehicleId,
        v.DriverId,
        v.DriverName,
        v.Status,
        v.CurrentYardId,
        ISNULL(y.YardName, 'Unassigned') AS CurrentYard,
        v.LastUpdatedUtc,
        v.EtaUtc,
        v.UtilizationPercent,
        r.RouteCode,
        CAST(CASE
            WHEN v.DriverId IS NOT NULL AND v.Status <> 'Delayed' THEN 1
            ELSE 0
        END AS BIT) AS IsDispatchReady
    FROM dbo.Vehicles v
    LEFT JOIN dbo.Yards y
        ON y.YardId = v.CurrentYardId
    LEFT JOIN dbo.Routes r
        ON r.RouteId = v.RouteId
    WHERE v.TenantId = @TenantId
      AND v.VehicleId = @VehicleId;
END;
