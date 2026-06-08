using NaviFreight.Api.Contracts;
using NaviFreight.Api.Models;
using NaviFreight.Api.Repositories;

namespace NaviFreight.Api.Services;

public sealed class SqlOperationsDataService(IOperationsRepository operationsRepository) : IOperationsDataService
{
    public Task<DashboardSummaryResponse> GetSummaryAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetSummaryAsync(tenantId, cancellationToken);

    public Task<IReadOnlyList<FleetVehicleResponse>> GetFleetAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetFleetAsync(tenantId, cancellationToken);

    public Task<FleetVehicleDetailResponse?> GetVehicleByIdAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
        => operationsRepository.GetVehicleByIdAsync(tenantId, vehicleId, cancellationToken);

    public Task<FleetVehicleDetailResponse> CreateVehicleAsync(string tenantId, CreateVehicleRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.CreateVehicleAsync(tenantId, request, cancellationToken);

    public Task<FleetVehicleDetailResponse?> UpdateVehicleAsync(string tenantId, string vehicleId, UpdateVehicleRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.UpdateVehicleAsync(tenantId, vehicleId, request, cancellationToken);

    public Task<bool> DeleteVehicleAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
        => operationsRepository.DeleteVehicleAsync(tenantId, vehicleId, cancellationToken);

    public Task<FleetVehicleDetailResponse?> AssignDriverAsync(string tenantId, string vehicleId, int driverId, CancellationToken cancellationToken = default)
        => operationsRepository.AssignDriverAsync(tenantId, vehicleId, driverId, cancellationToken);

    public Task<FleetVehicleDetailResponse?> UnassignDriverAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
        => operationsRepository.UnassignDriverAsync(tenantId, vehicleId, cancellationToken);

    public Task<IReadOnlyList<DriverResponse>> GetDriversAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetDriversAsync(tenantId, cancellationToken);

    public Task<DriverResponse?> GetDriverByIdAsync(string tenantId, int driverId, CancellationToken cancellationToken = default)
        => operationsRepository.GetDriverByIdAsync(tenantId, driverId, cancellationToken);

    public Task<DriverResponse> CreateDriverAsync(string tenantId, CreateDriverRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.CreateDriverAsync(tenantId, request, cancellationToken);

    public Task<DriverResponse?> UpdateDriverAsync(string tenantId, int driverId, UpdateDriverRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.UpdateDriverAsync(tenantId, driverId, request, cancellationToken);

    public Task<bool> DeleteDriverAsync(string tenantId, int driverId, CancellationToken cancellationToken = default)
        => operationsRepository.DeleteDriverAsync(tenantId, driverId, cancellationToken);

    public Task<IReadOnlyList<YardSnapshotResponse>> GetYardsAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetYardsAsync(tenantId, cancellationToken);

    public Task<IReadOnlyList<YardResponse>> GetYardListAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetYardListAsync(tenantId, cancellationToken);

    public Task<YardDetailResponse?> GetYardDetailAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
        => operationsRepository.GetYardDetailAsync(tenantId, yardId, cancellationToken);

    public Task<YardDetailResponse> CreateYardAsync(string tenantId, CreateYardRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.CreateYardAsync(tenantId, request, cancellationToken);

    public Task<YardResponse?> UpdateYardAsync(string tenantId, int yardId, UpdateYardRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.UpdateYardAsync(tenantId, yardId, request, cancellationToken);

    public Task<bool> DeleteYardAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
        => operationsRepository.DeleteYardAsync(tenantId, yardId, cancellationToken);

    public Task<YardResponse?> UpdateYardOperationalAsync(string tenantId, int yardId, YardOperationalUpdateRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.UpdateYardOperationalAsync(tenantId, yardId, request, cancellationToken);

    public Task<IReadOnlyList<DockResponse>> GetDocksAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
        => operationsRepository.GetDocksAsync(tenantId, yardId, cancellationToken);

    public Task<DockResponse?> GetDockDetailAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
        => operationsRepository.GetDockDetailAsync(tenantId, yardId, dockId, cancellationToken);

    public Task<DockResponse?> CreateDockAsync(string tenantId, int yardId, CreateDockRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.CreateDockAsync(tenantId, yardId, request, cancellationToken);

    public Task<DockResponse?> UpdateDockAsync(string tenantId, int yardId, int dockId, UpdateDockRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.UpdateDockAsync(tenantId, yardId, dockId, request, cancellationToken);

    public Task<bool> DeleteDockAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
        => operationsRepository.DeleteDockAsync(tenantId, yardId, dockId, cancellationToken);

    public Task<DockResponse?> AssignVehicleToDockAsync(string tenantId, int yardId, int dockId, string vehicleId, CancellationToken cancellationToken = default)
        => operationsRepository.AssignVehicleToDockAsync(tenantId, yardId, dockId, vehicleId, cancellationToken);

    public Task<DockResponse?> ReleaseDockAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
        => operationsRepository.ReleaseDockAsync(tenantId, yardId, dockId, cancellationToken);

    public Task<IReadOnlyList<RouteAssignmentResponse>> GetRoutesAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetRoutesAsync(tenantId, cancellationToken);

    public Task<RouteDetailResponse?> GetRouteByCodeAsync(string tenantId, string routeCode, CancellationToken cancellationToken = default)
        => operationsRepository.GetRouteByCodeAsync(tenantId, routeCode, cancellationToken);

    public Task<RouteAssignmentResponse> CreateRouteAsync(string tenantId, CreateRouteRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.CreateRouteAsync(tenantId, request, cancellationToken);

    public Task<RouteAssignmentResponse?> UpdateRouteAsync(string tenantId, string routeCode, UpdateRouteRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.UpdateRouteAsync(tenantId, routeCode, request, cancellationToken);

    public Task<bool> DeleteRouteAsync(string tenantId, string routeCode, CancellationToken cancellationToken = default)
        => operationsRepository.DeleteRouteAsync(tenantId, routeCode, cancellationToken);

    public Task<RouteDetailResponse?> AssignVehiclesAsync(string tenantId, string routeCode, IReadOnlyList<string> vehicleIds, CancellationToken cancellationToken = default)
        => operationsRepository.AssignVehiclesAsync(tenantId, routeCode, vehicleIds, cancellationToken);

    public Task<RouteDetailResponse?> UnassignVehicleAsync(string tenantId, string routeCode, string vehicleId, CancellationToken cancellationToken = default)
        => operationsRepository.UnassignVehicleAsync(tenantId, routeCode, vehicleId, cancellationToken);

    public Task<IReadOnlyList<AlertItemResponse>> GetAlertsAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetAlertsAsync(tenantId, cancellationToken);

    public Task<IReadOnlyList<AlertResponse>> GetAlertListAsync(string tenantId, string? status = null, string? severity = null, CancellationToken cancellationToken = default)
        => operationsRepository.GetAlertListAsync(tenantId, status, severity, cancellationToken);

    public Task<AlertResponse?> GetAlertDetailAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
        => operationsRepository.GetAlertDetailAsync(tenantId, alertId, cancellationToken);

    public Task<AlertResponse> CreateAlertAsync(string tenantId, CreateAlertRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.CreateAlertAsync(tenantId, request, cancellationToken);

    public Task<AlertResponse?> AcknowledgeAlertAsync(string tenantId, int alertId, AcknowledgeAlertRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.AcknowledgeAlertAsync(tenantId, alertId, request, cancellationToken);

    public Task<AlertResponse?> ResolveAlertAsync(string tenantId, int alertId, ResolveAlertRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.ResolveAlertAsync(tenantId, alertId, request, cancellationToken);

    public Task<AlertResponse?> ReopenAlertAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
        => operationsRepository.ReopenAlertAsync(tenantId, alertId, cancellationToken);

    public Task<AlertResponse?> UpdateAlertOwnerAsync(string tenantId, int alertId, UpdateAlertOwnerRequest request, CancellationToken cancellationToken = default)
        => operationsRepository.UpdateAlertOwnerAsync(tenantId, alertId, request, cancellationToken);

    public Task<bool> CloseAlertAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
        => operationsRepository.CloseAlertAsync(tenantId, alertId, cancellationToken);

    public Task<IReadOnlyList<ReportSnapshotResponse>> GetReportsAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetReportsAsync(tenantId, cancellationToken);

    public Task<IReadOnlyList<SettingsSectionResponse>> GetSettingsAsync(string tenantId, CancellationToken cancellationToken = default)
        => operationsRepository.GetSettingsAsync(tenantId, cancellationToken);

    public async Task<OperationalOverviewResponse> GetOverviewAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, cancellationToken);
        var fleet = await GetFleetAsync(tenantId, cancellationToken);
        var yards = await GetYardsAsync(tenantId, cancellationToken);
        var routes = await GetRoutesAsync(tenantId, cancellationToken);
        var alerts = await GetAlertsAsync(tenantId, cancellationToken);
        var reports = await GetReportsAsync(tenantId, cancellationToken);
        var settings = await GetSettingsAsync(tenantId, cancellationToken);

        return new OperationalOverviewResponse(summary, fleet, yards, routes, alerts, reports, settings);
    }
}
