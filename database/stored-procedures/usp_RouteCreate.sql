CREATE OR ALTER PROCEDURE dbo.usp_RouteCreate
    @TenantId NVARCHAR(50),
    @RouteCode NVARCHAR(50),
    @OriginYardId INT,
    @DestinationName NVARCHAR(150),
    @Status NVARCHAR(50),
    @NextDepartureUtc DATETIME2,
    @CompletionPercent INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Routes (
        TenantId,
        RouteCode,
        OriginYardId,
        DestinationName,
        Status,
        NextDepartureUtc,
        CompletionPercent)
    VALUES (
        @TenantId,
        @RouteCode,
        @OriginYardId,
        @DestinationName,
        @Status,
        @NextDepartureUtc,
        @CompletionPercent);

    SELECT
        r.RouteCode,
        y.YardName AS Origin,
        r.DestinationName AS Destination,
        r.Status,
        0 AS AssignedVehicles,
        r.NextDepartureUtc,
        r.CompletionPercent
    FROM dbo.Routes r
    INNER JOIN dbo.Yards y
        ON y.YardId = r.OriginYardId
    WHERE r.TenantId = @TenantId
      AND r.RouteCode = @RouteCode;
END;
