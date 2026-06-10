namespace NaviFreight.Api.Contracts;

public sealed record ReportSummaryResponse(
    DateTime From,
    DateTime To,
    // Alerts
    int TotalAlerts,
    int CriticalAlerts,
    int WarningAlerts,
    int InfoAlerts,
    // Fleet
    int TotalVehicles,
    int InTransitVehicles,
    int AtDockVehicles,
    int AwaitingDispatchVehicles,
    int DelayedVehicles,
    // Routes
    int TotalRoutes,
    int ActiveRoutes,
    int OnScheduleRoutes,
    // Yards
    int TotalYards,
    int AvgYardOccupancyPercent);
