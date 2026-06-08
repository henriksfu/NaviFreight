namespace NaviFreight.Api.Contracts;

public sealed record DashboardSummaryResponse(
    string TenantId,
    int ActiveVehicles,
    int YardOccupancyPercent,
    int DelayedLoads,
    decimal OnTimeDispatchRate,
    int ActiveRoutes,
    int TrailerTurnaroundMinutes);
