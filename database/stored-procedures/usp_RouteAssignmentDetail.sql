CREATE OR ALTER PROCEDURE dbo.usp_RouteAssignmentDetail
    @TenantId NVARCHAR(50),
    @RouteCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.RouteCode,
        y.YardName AS Origin,
        r.DestinationName AS Destination,
        r.Status,
        (
            SELECT COUNT(*)
            FROM dbo.Vehicles vx
            WHERE vx.RouteId = r.RouteId
        ) AS AssignedVehicles,
        r.NextDepartureUtc,
        r.CompletionPercent,
        v.VehicleId,
        v.DriverName,
        v.Status AS VehicleStatus,
        y2.YardName AS CurrentYard,
        v.EtaUtc
    FROM dbo.Routes r
    INNER JOIN dbo.Yards y
        ON y.YardId = r.OriginYardId
    LEFT JOIN dbo.Vehicles v
        ON v.RouteId = r.RouteId
    LEFT JOIN dbo.Yards y2
        ON y2.YardId = v.CurrentYardId
    WHERE r.TenantId = @TenantId
      AND r.RouteCode = @RouteCode;
END;
