using NaviFreight.Api.Contracts;
using NaviFreight.Api.Models;

namespace NaviFreight.Api.Repositories;

public interface IOperationsRepository
{
    Task<DashboardSummaryResponse> GetSummaryAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FleetVehicleResponse>> GetFleetAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<FleetVehicleDetailResponse?> GetVehicleByIdAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default);
    Task<FleetVehicleDetailResponse> CreateVehicleAsync(string tenantId, CreateVehicleRequest request, CancellationToken cancellationToken = default);
    Task<FleetVehicleDetailResponse?> UpdateVehicleAsync(string tenantId, string vehicleId, UpdateVehicleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteVehicleAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default);
    Task<FleetVehicleDetailResponse?> AssignDriverAsync(string tenantId, string vehicleId, int driverId, CancellationToken cancellationToken = default);
    Task<FleetVehicleDetailResponse?> UnassignDriverAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DriverResponse>> GetDriversAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<DriverResponse?> GetDriverByIdAsync(string tenantId, int driverId, CancellationToken cancellationToken = default);
    Task<DriverResponse> CreateDriverAsync(string tenantId, CreateDriverRequest request, CancellationToken cancellationToken = default);
    Task<DriverResponse?> UpdateDriverAsync(string tenantId, int driverId, UpdateDriverRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteDriverAsync(string tenantId, int driverId, CancellationToken cancellationToken = default);

    // Yards — snapshot view
    Task<IReadOnlyList<YardSnapshotResponse>> GetYardsAsync(string tenantId, CancellationToken cancellationToken = default);

    // Yards — full CRUD
    Task<IReadOnlyList<YardResponse>> GetYardListAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<YardDetailResponse?> GetYardDetailAsync(string tenantId, int yardId, CancellationToken cancellationToken = default);
    Task<YardDetailResponse> CreateYardAsync(string tenantId, CreateYardRequest request, CancellationToken cancellationToken = default);
    Task<YardResponse?> UpdateYardAsync(string tenantId, int yardId, UpdateYardRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteYardAsync(string tenantId, int yardId, CancellationToken cancellationToken = default);
    Task<YardResponse?> UpdateYardOperationalAsync(string tenantId, int yardId, YardOperationalUpdateRequest request, CancellationToken cancellationToken = default);

    // Docks
    Task<IReadOnlyList<DockResponse>> GetDocksAsync(string tenantId, int yardId, CancellationToken cancellationToken = default);
    Task<DockResponse?> GetDockDetailAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default);
    Task<DockResponse?> CreateDockAsync(string tenantId, int yardId, CreateDockRequest request, CancellationToken cancellationToken = default);
    Task<DockResponse?> UpdateDockAsync(string tenantId, int yardId, int dockId, UpdateDockRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteDockAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default);
    Task<DockResponse?> AssignVehicleToDockAsync(string tenantId, int yardId, int dockId, string vehicleId, CancellationToken cancellationToken = default);
    Task<DockResponse?> ReleaseDockAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RouteAssignmentResponse>> GetRoutesAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<RouteDetailResponse?> GetRouteByCodeAsync(string tenantId, string routeCode, CancellationToken cancellationToken = default);
    Task<RouteAssignmentResponse> CreateRouteAsync(string tenantId, CreateRouteRequest request, CancellationToken cancellationToken = default);
    Task<RouteAssignmentResponse?> UpdateRouteAsync(string tenantId, string routeCode, UpdateRouteRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRouteAsync(string tenantId, string routeCode, CancellationToken cancellationToken = default);
    Task<RouteDetailResponse?> AssignVehiclesAsync(string tenantId, string routeCode, IReadOnlyList<string> vehicleIds, CancellationToken cancellationToken = default);
    Task<RouteDetailResponse?> UnassignVehicleAsync(string tenantId, string routeCode, string vehicleId, CancellationToken cancellationToken = default);

    // Alerts — summary view
    Task<IReadOnlyList<AlertItemResponse>> GetAlertsAsync(string tenantId, CancellationToken cancellationToken = default);

    // Alerts — full CRUD + lifecycle
    Task<IReadOnlyList<AlertResponse>> GetAlertListAsync(string tenantId, string? status = null, string? severity = null, CancellationToken cancellationToken = default);
    Task<AlertResponse?> GetAlertDetailAsync(string tenantId, int alertId, CancellationToken cancellationToken = default);
    Task<AlertResponse> CreateAlertAsync(string tenantId, CreateAlertRequest request, CancellationToken cancellationToken = default);
    Task<AlertResponse?> AcknowledgeAlertAsync(string tenantId, int alertId, AcknowledgeAlertRequest request, CancellationToken cancellationToken = default);
    Task<AlertResponse?> ResolveAlertAsync(string tenantId, int alertId, ResolveAlertRequest request, CancellationToken cancellationToken = default);
    Task<AlertResponse?> ReopenAlertAsync(string tenantId, int alertId, CancellationToken cancellationToken = default);
    Task<AlertResponse?> UpdateAlertOwnerAsync(string tenantId, int alertId, UpdateAlertOwnerRequest request, CancellationToken cancellationToken = default);
    Task<bool> CloseAlertAsync(string tenantId, int alertId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReportSnapshotResponse>> GetReportsAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SettingsSectionResponse>> GetSettingsAsync(string tenantId, CancellationToken cancellationToken = default);

    // Settings (editable)
    Task<IReadOnlyList<TenantSettingResponse>> GetTenantSettingsAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<TenantSettingResponse> UpdateTenantSettingAsync(string tenantId, string key, string value, CancellationToken cancellationToken = default);

    // Reports summary
    Task<ReportSummaryResponse> GetReportSummaryAsync(string tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    // Users
    Task<IReadOnlyList<UserResponse>> GetUsersAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateUserAsync(string tenantId, CreateUserRequest request, string passwordHash, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateUserAsync(string tenantId, int userId, UpdateUserRequest request, string? newPasswordHash, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(string tenantId, int userId, CancellationToken cancellationToken = default);
    Task<UserResponse?> ReactivateUserAsync(string tenantId, int userId, CancellationToken cancellationToken = default);
}
