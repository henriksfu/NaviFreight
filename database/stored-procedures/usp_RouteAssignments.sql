CREATE OR ALTER PROCEDURE dbo.usp_RouteAssignments
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.RouteCode,
        y.YardName AS Origin,
        r.DestinationName AS Destination,
        r.Status,
        COUNT(v.VehicleId) AS AssignedVehicles,
        r.NextDepartureUtc,
        r.CompletionPercent
    FROM dbo.Routes r
    INNER JOIN dbo.Yards y
        ON y.YardId = r.OriginYardId
    LEFT JOIN dbo.Vehicles v
        ON v.RouteId = r.RouteId
    WHERE r.TenantId = @TenantId
    GROUP BY
        r.RouteCode,
        y.YardName,
        r.DestinationName,
        r.Status,
        r.NextDepartureUtc,
        r.CompletionPercent
    ORDER BY r.NextDepartureUtc;
END;
