CREATE OR ALTER PROCEDURE dbo.usp_FleetVehicles
    @TenantId  NVARCHAR(50),
    @Page      INT = 1,
    @PageSize  INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT
        COUNT(*) OVER()                                                          AS TotalCount,
        v.VehicleId,
        v.DriverName,
        v.Status,
        y.YardName                                                               AS CurrentYard,
        v.LastUpdatedUtc,
        v.EtaUtc,
        v.UtilizationPercent,
        r.RouteCode
    FROM dbo.Vehicles v
    LEFT JOIN dbo.Yards y  ON y.YardId  = v.CurrentYardId
    LEFT JOIN dbo.Routes r ON r.RouteId = v.RouteId
    WHERE v.TenantId = @TenantId
    ORDER BY v.LastUpdatedUtc DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
