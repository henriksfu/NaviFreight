CREATE OR ALTER PROCEDURE dbo.usp_RouteUpdate
    @TenantId NVARCHAR(50),
    @RouteCode NVARCHAR(50),
    @DestinationName NVARCHAR(150),
    @Status NVARCHAR(50),
    @NextDepartureUtc DATETIME2,
    @CompletionPercent INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Routes
    SET DestinationName = @DestinationName,
        Status = @Status,
        NextDepartureUtc = @NextDepartureUtc,
        CompletionPercent = @CompletionPercent
    WHERE TenantId = @TenantId
      AND RouteCode = @RouteCode;

    IF @@ROWCOUNT = 0
    BEGIN
        RETURN;
    END;

    SELECT
        r.RouteCode,
        y.YardName AS Origin,
        r.DestinationName AS Destination,
        r.Status,
        (
            SELECT COUNT(*)
            FROM dbo.Vehicles v
            WHERE v.RouteId = r.RouteId
        ) AS AssignedVehicles,
        r.NextDepartureUtc,
        r.CompletionPercent
    FROM dbo.Routes r
    INNER JOIN dbo.Yards y
        ON y.YardId = r.OriginYardId
    WHERE r.TenantId = @TenantId
      AND r.RouteCode = @RouteCode;
END;
