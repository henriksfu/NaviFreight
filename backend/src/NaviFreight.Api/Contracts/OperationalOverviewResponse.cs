namespace NaviFreight.Api.Contracts;

public sealed record OperationalOverviewResponse(
    DashboardSummaryResponse Summary,
    IReadOnlyList<FleetVehicleResponse> Fleet,
    IReadOnlyList<YardSnapshotResponse> Yards,
    IReadOnlyList<RouteAssignmentResponse> Routes,
    IReadOnlyList<AlertItemResponse> Alerts,
    IReadOnlyList<ReportSnapshotResponse> Reports,
    IReadOnlyList<SettingsSectionResponse> Settings);
