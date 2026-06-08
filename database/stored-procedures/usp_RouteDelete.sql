CREATE OR ALTER PROCEDURE dbo.usp_RouteDelete
    @TenantId NVARCHAR(50),
    @RouteCode NVARCHAR(50)
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
        RETURN;
    END;

    UPDATE dbo.Vehicles
    SET RouteId = NULL
    WHERE TenantId = @TenantId
      AND RouteId = @RouteId;

    DELETE FROM dbo.Routes
    WHERE RouteId = @RouteId;
END;
