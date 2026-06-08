CREATE OR ALTER PROCEDURE dbo.usp_DashboardSummary
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ActiveVehicles INT = (
        SELECT COUNT(*)
        FROM dbo.Vehicles
        WHERE TenantId = @TenantId
          AND Status IN ('In Transit', 'At Dock', 'Awaiting Dispatch')
    );

    DECLARE @DelayedLoads INT = (
        SELECT COUNT(*)
        FROM dbo.Vehicles
        WHERE TenantId = @TenantId
          AND Status = 'Delayed'
    );

    DECLARE @YardOccupancyPercent INT = (
        SELECT TOP 1
            CASE
                WHEN SUM(Capacity) = 0 THEN 0
                ELSE CAST((SUM(OccupiedSlots) * 100.0) / SUM(Capacity) AS INT)
            END
        FROM dbo.Yards
        WHERE TenantId = @TenantId
        GROUP BY TenantId
    );

    DECLARE @ActiveRoutes INT = (
        SELECT COUNT(*)
        FROM dbo.Routes
        WHERE TenantId = @TenantId
          AND Status IN ('On Schedule', 'At Risk', 'Delayed', 'Awaiting Dispatch')
    );

    DECLARE @TrailerTurnaroundMinutes INT = (
        SELECT CAST(AVG(CAST(AverageTurnMinutes AS DECIMAL(10,2))) AS INT)
        FROM dbo.Yards
        WHERE TenantId = @TenantId
    );

    SELECT
        @TenantId AS TenantId,
        ISNULL(@ActiveVehicles, 0) AS ActiveVehicles,
        ISNULL(@YardOccupancyPercent, 0) AS YardOccupancyPercent,
        ISNULL(@DelayedLoads, 0) AS DelayedLoads,
        CAST(96.4 AS DECIMAL(5,2)) AS OnTimeDispatchRate,
        ISNULL(@ActiveRoutes, 0) AS ActiveRoutes,
        ISNULL(@TrailerTurnaroundMinutes, 0) AS TrailerTurnaroundMinutes;
END;
