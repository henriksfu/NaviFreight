CREATE OR ALTER PROCEDURE dbo.usp_ReportSummary
    @TenantId NVARCHAR(50),
    @From     DATETIME2,
    @To       DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Alerts in period
    SELECT
        COUNT(*)                                                        AS TotalAlerts,
        SUM(CASE WHEN Severity = 'Critical' THEN 1 ELSE 0 END)        AS CriticalAlerts,
        SUM(CASE WHEN Severity = 'Warning'  THEN 1 ELSE 0 END)        AS WarningAlerts,
        SUM(CASE WHEN Severity = 'Info'     THEN 1 ELSE 0 END)        AS InfoAlerts
    FROM dbo.AlertEvents
    WHERE TenantId = @TenantId
      AND CreatedUtc BETWEEN @From AND @To;

    -- Fleet snapshot
    SELECT
        COUNT(*)                                                                    AS TotalVehicles,
        SUM(CASE WHEN VehicleStatus = 'In Transit'         THEN 1 ELSE 0 END)     AS InTransit,
        SUM(CASE WHEN VehicleStatus = 'At Dock'            THEN 1 ELSE 0 END)     AS AtDock,
        SUM(CASE WHEN VehicleStatus = 'Awaiting Dispatch'  THEN 1 ELSE 0 END)     AS AwaitingDispatch,
        SUM(CASE WHEN VehicleStatus = 'Delayed'            THEN 1 ELSE 0 END)     AS Delayed
    FROM dbo.Vehicles
    WHERE TenantId = @TenantId;

    -- Routes snapshot
    SELECT
        COUNT(*)                                                              AS TotalRoutes,
        SUM(CASE WHEN RouteStatus != 'Completed' THEN 1 ELSE 0 END)         AS ActiveRoutes,
        SUM(CASE WHEN RouteStatus = 'On Schedule' THEN 1 ELSE 0 END)        AS OnSchedule
    FROM dbo.RouteAssignments
    WHERE TenantId = @TenantId;

    -- Yard occupancy
    SELECT
        COUNT(*)                                         AS TotalYards,
        AVG(CAST(OccupiedSlots AS FLOAT) / NULLIF(CAST(TotalCapacity AS FLOAT), 0) * 100) AS AvgOccupancyPct
    FROM dbo.Yards
    WHERE TenantId = @TenantId AND IsActive = 1;
END;
GO
