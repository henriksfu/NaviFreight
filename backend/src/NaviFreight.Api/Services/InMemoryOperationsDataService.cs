using NaviFreight.Api.Contracts;
using NaviFreight.Api.Models;

namespace NaviFreight.Api.Services;

public sealed class InMemoryOperationsDataService : IOperationsDataService
{
    public Task<DashboardSummaryResponse> GetSummaryAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new DashboardSummaryResponse(
            tenantId,
            ActiveVehicles: 184,
            YardOccupancyPercent: 72,
            DelayedLoads: 14,
            OnTimeDispatchRate: 96.4m,
            ActiveRoutes: 28,
            TrailerTurnaroundMinutes: 44));
    }

    private IReadOnlyList<FleetVehicleResponse> AllFleet() =>
    [
        new("VH-1042", "Ava Patel",   "In Transit",       "Seattle North",   DateTime.UtcNow.AddMinutes(-8),  DateTime.UtcNow.AddHours(2), 89, "NW-14"),
        new("VH-1188", "Marcus Gray", "At Dock",           "Portland East",   DateTime.UtcNow.AddMinutes(-15), null,                        74, "OR-07"),
        new("VH-1211", "Nina Chen",   "Awaiting Dispatch", "Seattle North",   DateTime.UtcNow.AddMinutes(-4),  DateTime.UtcNow.AddHours(3), 62, "PN-22"),
        new("VH-1305", "Ethan Ross",  "Delayed",           "Portland East",   DateTime.UtcNow.AddMinutes(-22), DateTime.UtcNow.AddHours(4), 51, "OR-03"),
        new("VH-1417", "Lena Brooks", "In Transit",        "Oakland Gateway", DateTime.UtcNow.AddMinutes(-2),  DateTime.UtcNow.AddHours(1), 93, "CA-11")
    ];

    public Task<PagedResponse<FleetVehicleResponse>> GetFleetAsync(string tenantId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var all = AllFleet();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResponse<FleetVehicleResponse>(items, all.Count, page, pageSize));
    }

    public async Task<FleetVehicleDetailResponse?> GetVehicleByIdAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
    {
        var vehicle = AllFleet()
            .FirstOrDefault(item => item.VehicleId.Equals(vehicleId, StringComparison.OrdinalIgnoreCase));

        return vehicle is null
            ? null
            : new FleetVehicleDetailResponse(
                vehicle.VehicleId,
                null,
                vehicle.DriverName,
                vehicle.Status,
                null,
                vehicle.CurrentYard,
                vehicle.LastUpdatedUtc,
                vehicle.EtaUtc,
                vehicle.UtilizationPercent,
                vehicle.RouteCode,
                !string.IsNullOrWhiteSpace(vehicle.DriverName) && vehicle.Status != "Delayed");
    }

    public async Task<FleetVehicleDetailResponse> CreateVehicleAsync(string tenantId, CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        return (await GetVehicleByIdAsync(tenantId, request.VehicleId, cancellationToken))
            ?? new FleetVehicleDetailResponse(
                request.VehicleId,
                null,
                "Unassigned",
                request.Status,
                request.CurrentYardId,
                request.CurrentYardId is null ? "Unassigned" : $"Yard {request.CurrentYardId}",
                DateTime.UtcNow,
                request.EtaUtc,
                request.UtilizationPercent,
                string.Empty,
                false);
    }

    public async Task<FleetVehicleDetailResponse?> UpdateVehicleAsync(string tenantId, string vehicleId, UpdateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        var current = await GetVehicleByIdAsync(tenantId, vehicleId, cancellationToken);
        if (current is null)
        {
            return null;
        }

        return current with
        {
            Status = request.Status,
            CurrentYardId = request.CurrentYardId,
            CurrentYard = request.CurrentYardId is null ? "Unassigned" : $"Yard {request.CurrentYardId}",
            EtaUtc = request.EtaUtc,
            UtilizationPercent = request.UtilizationPercent,
            LastUpdatedUtc = DateTime.UtcNow
        };
    }

    public Task<bool> DeleteVehicleAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public async Task<FleetVehicleDetailResponse?> AssignDriverAsync(string tenantId, string vehicleId, int driverId, CancellationToken cancellationToken = default)
    {
        var current = await GetVehicleByIdAsync(tenantId, vehicleId, cancellationToken);
        var driver = await GetDriverByIdAsync(tenantId, driverId, cancellationToken);

        if (current is null || driver is null)
        {
            return null;
        }

        return current with
        {
            DriverId = driver.DriverId,
            DriverName = driver.FullName,
            IsDispatchReady = driver.AvailabilityStatus != "Unavailable"
        };
    }

    public async Task<FleetVehicleDetailResponse?> UnassignDriverAsync(string tenantId, string vehicleId, CancellationToken cancellationToken = default)
    {
        var current = await GetVehicleByIdAsync(tenantId, vehicleId, cancellationToken);
        return current is null
            ? null
            : current with { DriverId = null, DriverName = "Unassigned", IsDispatchReady = false };
    }

    private IReadOnlyList<DriverResponse> AllDrivers() =>
    [
        new(1, "Ava Patel",   "DL-SEA-1004", "Assigned", "VH-1042"),
        new(2, "Marcus Gray", "DL-PDX-4411", "Assigned", "VH-1188"),
        new(3, "Nina Chen",   "DL-SEA-8821", "Standby",  "VH-1211"),
        new(4, "Ethan Ross",  "DL-PDX-1990", "Delayed",  "VH-1305"),
        new(5, "Lena Brooks", "DL-OAK-5518", "Assigned", "VH-1417")
    ];

    public Task<PagedResponse<DriverResponse>> GetDriversAsync(string tenantId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var all = AllDrivers();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResponse<DriverResponse>(items, all.Count, page, pageSize));
    }

    public Task<DriverResponse?> GetDriverByIdAsync(string tenantId, int driverId, CancellationToken cancellationToken = default)
        => Task.FromResult(AllDrivers().FirstOrDefault(item => item.DriverId == driverId));

    public Task<DriverResponse> CreateDriverAsync(string tenantId, CreateDriverRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new DriverResponse(99, request.FullName, request.LicenseNumber, request.AvailabilityStatus, null));

    public async Task<DriverResponse?> UpdateDriverAsync(string tenantId, int driverId, UpdateDriverRequest request, CancellationToken cancellationToken = default)
    {
        var current = await GetDriverByIdAsync(tenantId, driverId, cancellationToken);
        return current is null ? null : current with
        {
            FullName = request.FullName,
            LicenseNumber = request.LicenseNumber,
            AvailabilityStatus = request.AvailabilityStatus
        };
    }

    public Task<bool> DeleteDriverAsync(string tenantId, int driverId, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    // ── Yards ─────────────────────────────────────────────────────────────────

    public Task<IReadOnlyList<YardSnapshotResponse>> GetYardsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<YardSnapshotResponse> items =
        [
            new("Seattle North", 83, 120, 6, 2, 38),
            new("Portland East", 42, 75, 3, 4, 46),
            new("Oakland Gateway", 65, 90, 8, 1, 49)
        ];
        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<YardResponse>> GetYardListAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<YardResponse> items =
        [
            new(1, "Seattle North",   120, 83, 6, 38, 3, 2, true, DateTime.UtcNow.AddDays(-90), null),
            new(2, "Portland East",    75, 42, 3, 46, 3, 4, true, DateTime.UtcNow.AddDays(-60), null),
            new(3, "Oakland Gateway",  90, 65, 8, 49, 3, 1, true, DateTime.UtcNow.AddDays(-30), null)
        ];
        return Task.FromResult(items);
    }

    public Task<YardDetailResponse?> GetYardDetailAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
    {
        var yards = new Dictionary<int, (YardResponse Yard, IReadOnlyList<DockResponse> Docks)>
        {
            [1] = (
                new(1, "Seattle North", 120, 83, 6, 38, 3, 2, true, DateTime.UtcNow.AddDays(-90), null),
                [
                    new(1, 1, "SEA-D1", "Occupied",  "VH-1042", null, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddMinutes(-8)),
                    new(2, 1, "SEA-D2", "Available", null,      null, DateTime.UtcNow.AddDays(-90), null),
                    new(3, 1, "SEA-D3", "Available", null,      null, DateTime.UtcNow.AddDays(-90), null)
                ]),
            [2] = (
                new(2, "Portland East", 75, 42, 3, 46, 3, 4, true, DateTime.UtcNow.AddDays(-60), null),
                [
                    new(4, 2, "PDX-D1", "Occupied",    "VH-1188", null, DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddMinutes(-15)),
                    new(5, 2, "PDX-D2", "Maintenance", null,      "Annual service", DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-1)),
                    new(6, 2, "PDX-D3", "Available",   null,      null, DateTime.UtcNow.AddDays(-60), null)
                ]),
            [3] = (
                new(3, "Oakland Gateway", 90, 65, 8, 49, 3, 1, true, DateTime.UtcNow.AddDays(-30), null),
                [
                    new(7, 3, "OAK-D1", "Available", null,      null, DateTime.UtcNow.AddDays(-30), null),
                    new(8, 3, "OAK-D2", "Available", null,      null, DateTime.UtcNow.AddDays(-30), null),
                    new(9, 3, "OAK-D3", "Occupied",  "VH-1417", null, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddMinutes(-2))
                ])
        };

        if (!yards.TryGetValue(yardId, out var entry))
            return Task.FromResult<YardDetailResponse?>(null);

        return Task.FromResult<YardDetailResponse?>(new YardDetailResponse(entry.Yard, entry.Docks));
    }

    public Task<YardDetailResponse> CreateYardAsync(string tenantId, CreateYardRequest request, CancellationToken cancellationToken = default)
    {
        var yard = new YardResponse(99, request.YardName, request.Capacity, 0, 0, 0, 0, 0, true, DateTime.UtcNow, null);
        return Task.FromResult(new YardDetailResponse(yard, []));
    }

    public async Task<YardResponse?> UpdateYardAsync(string tenantId, int yardId, UpdateYardRequest request, CancellationToken cancellationToken = default)
    {
        var detail = await GetYardDetailAsync(tenantId, yardId, cancellationToken);
        return detail is null ? null : detail.Yard with { YardName = request.YardName, Capacity = request.Capacity, UpdatedUtc = DateTime.UtcNow };
    }

    public Task<bool> DeleteYardAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public async Task<YardResponse?> UpdateYardOperationalAsync(string tenantId, int yardId, YardOperationalUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var detail = await GetYardDetailAsync(tenantId, yardId, cancellationToken);
        return detail is null ? null : detail.Yard with
        {
            OccupiedSlots = request.OccupiedSlots,
            InboundQueue = request.InboundQueue,
            AverageTurnMinutes = request.AverageTurnMinutes,
            UpdatedUtc = DateTime.UtcNow
        };
    }

    // ── Docks ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DockResponse>> GetDocksAsync(string tenantId, int yardId, CancellationToken cancellationToken = default)
    {
        var detail = await GetYardDetailAsync(tenantId, yardId, cancellationToken);
        return detail?.Docks ?? [];
    }

    public async Task<DockResponse?> GetDockDetailAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
    {
        var docks = await GetDocksAsync(tenantId, yardId, cancellationToken);
        return docks.FirstOrDefault(d => d.DockId == dockId);
    }

    public Task<DockResponse?> CreateDockAsync(string tenantId, int yardId, CreateDockRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<DockResponse?>(new(99, yardId, request.DockCode, request.Status, null, null, DateTime.UtcNow, null));
    }

    public async Task<DockResponse?> UpdateDockAsync(string tenantId, int yardId, int dockId, UpdateDockRequest request, CancellationToken cancellationToken = default)
    {
        var dock = await GetDockDetailAsync(tenantId, yardId, dockId, cancellationToken);
        return dock is null ? null : dock with { DockCode = request.DockCode, Status = request.Status, Notes = request.Notes, UpdatedUtc = DateTime.UtcNow };
    }

    public Task<bool> DeleteDockAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public async Task<DockResponse?> AssignVehicleToDockAsync(string tenantId, int yardId, int dockId, string vehicleId, CancellationToken cancellationToken = default)
    {
        var dock = await GetDockDetailAsync(tenantId, yardId, dockId, cancellationToken);
        return dock is null ? null : dock with { Status = "Occupied", OccupyingVehicleId = vehicleId, UpdatedUtc = DateTime.UtcNow };
    }

    public async Task<DockResponse?> ReleaseDockAsync(string tenantId, int yardId, int dockId, CancellationToken cancellationToken = default)
    {
        var dock = await GetDockDetailAsync(tenantId, yardId, dockId, cancellationToken);
        return dock is null ? null : dock with { Status = "Available", OccupyingVehicleId = null, UpdatedUtc = DateTime.UtcNow };
    }

    // ── Routes ────────────────────────────────────────────────────────────────

    private IReadOnlyList<RouteAssignmentResponse> AllRoutes() =>
    [
        new("NW-14", "Seattle North",   "Spokane Hub",       "On Schedule", 5, DateTime.UtcNow.AddMinutes(35),  68),
        new("OR-03", "Portland East",   "Boise Crossdock",   "Delayed",     3, DateTime.UtcNow.AddMinutes(75),  42),
        new("CA-11", "Oakland Gateway", "Reno Freight Park", "At Risk",     4, DateTime.UtcNow.AddMinutes(130), 57)
    ];

    public Task<PagedResponse<RouteAssignmentResponse>> GetRoutesAsync(string tenantId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var all = AllRoutes();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResponse<RouteAssignmentResponse>(items, all.Count, page, pageSize));
    }

    public Task<RouteDetailResponse?> GetRouteByCodeAsync(string tenantId, string routeCode, CancellationToken cancellationToken = default)
    {
        var route = AllRoutes()
            .FirstOrDefault(item => item.RouteCode.Equals(routeCode, StringComparison.OrdinalIgnoreCase));

        if (route is null)
        {
            return null;
        }

        var assignedVehicles = AllFleet()
            .Where(vehicle => vehicle.RouteCode.Equals(routeCode, StringComparison.OrdinalIgnoreCase))
            .Select(vehicle => new RouteDispatchAssignmentResponse(
                vehicle.VehicleId,
                vehicle.DriverName,
                vehicle.Status,
                vehicle.CurrentYard,
                vehicle.EtaUtc))
            .ToList();

        return new RouteDetailResponse(route, assignedVehicles);
    }

    public Task<RouteAssignmentResponse> CreateRouteAsync(string tenantId, CreateRouteRequest request, CancellationToken cancellationToken = default)
    {
        var created = new RouteAssignmentResponse(
            request.RouteCode,
            $"Yard {request.OriginYardId}",
            request.DestinationName,
            request.Status,
            0,
            request.NextDepartureUtc,
            request.CompletionPercent);

        return Task.FromResult(created);
    }

    public Task<RouteAssignmentResponse?> UpdateRouteAsync(string tenantId, string routeCode, UpdateRouteRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<RouteAssignmentResponse?>(new(
            routeCode,
            "Updated Origin",
            request.DestinationName,
            request.Status,
            0,
            request.NextDepartureUtc,
            request.CompletionPercent));
    }

    public Task<bool> DeleteRouteAsync(string tenantId, string routeCode, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public async Task<RouteDetailResponse?> AssignVehiclesAsync(string tenantId, string routeCode, IReadOnlyList<string> vehicleIds, CancellationToken cancellationToken = default)
        => await GetRouteByCodeAsync(tenantId, routeCode, cancellationToken);

    public async Task<RouteDetailResponse?> UnassignVehicleAsync(string tenantId, string routeCode, string vehicleId, CancellationToken cancellationToken = default)
        => await GetRouteByCodeAsync(tenantId, routeCode, cancellationToken);

    // ── Alerts ────────────────────────────────────────────────────────────────

    public Task<IReadOnlyList<AlertItemResponse>> GetAlertsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AlertItemResponse> items =
        [
            new("Critical", "Trailer queue exceeding target", "Oakland Gateway has remained above 7 inbound trailers for 35 minutes.", "Yard Ops"),
            new("Warning", "Dispatch SLA drift", "Northwest outbound wave is 9 minutes behind target dispatch time.", "Routing"),
            new("Info", "Fleet sync completed", "Global telematics reconciliation completed without dropped assets.", "Platform")
        ];
        return Task.FromResult(items);
    }

    private IReadOnlyList<AlertResponse> AllAlerts()
    {
        var now = DateTime.UtcNow;
        return
        [
            new(1, "Critical", "Trailer queue exceeding target",
                "Oakland Gateway has remained above 7 inbound trailers for 35 minutes.",
                "Yard Ops", "Active", true, now.AddMinutes(-35),
                null, null, null, null, null, null),
            new(2, "Warning", "Dispatch SLA drift",
                "Northwest outbound wave is 9 minutes behind target dispatch time.",
                "Routing", "Acknowledged", true, now.AddMinutes(-90),
                "priya.shah@atlasmeridian.example", now.AddMinutes(-80),
                null, null, null, now.AddMinutes(-80)),
            new(3, "Info", "Fleet sync completed",
                "Global telematics reconciliation completed without dropped assets.",
                "Platform", "Resolved", false, now.AddHours(-2),
                null, null,
                "morgan.ellis@atlasmeridian.example", now.AddHours(-1),
                "Completed successfully during maintenance window.", now.AddHours(-1)),
            new(4, "Warning", "Dock recompute latency",
                "Portland East dock recompute cycle exceeded 120 second threshold.",
                "Platform", "Active", true, now.AddMinutes(-12),
                null, null, null, null, null, null)
        ];
    }

    public Task<PagedResponse<AlertResponse>> GetAlertListAsync(string tenantId, string? status = null, string? severity = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var filtered = AllAlerts().AsEnumerable();
        if (status is not null)
            filtered = filtered.Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        if (severity is not null)
            filtered = filtered.Where(a => a.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase));

        var list = filtered.ToList();
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResponse<AlertResponse>(items, list.Count, page, pageSize));
    }

    public Task<AlertResponse?> GetAlertDetailAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
        => Task.FromResult(AllAlerts().FirstOrDefault(a => a.AlertEventId == alertId));

    public Task<AlertResponse> CreateAlertAsync(string tenantId, CreateAlertRequest request, CancellationToken cancellationToken = default)
    {
        var alert = new AlertResponse(
            99, request.Severity, request.Title, request.Description, request.OwnerTeam,
            "Active", true, DateTime.UtcNow,
            null, null, null, null, null, null);
        return Task.FromResult(alert);
    }

    public async Task<AlertResponse?> AcknowledgeAlertAsync(string tenantId, int alertId, AcknowledgeAlertRequest request, CancellationToken cancellationToken = default)
    {
        var alert = await GetAlertDetailAsync(tenantId, alertId, cancellationToken);
        if (alert is null || alert.Status != "Active") return null;

        return alert with
        {
            Status = "Acknowledged",
            AcknowledgedByEmail = request.AcknowledgedByEmail,
            AcknowledgedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
    }

    public async Task<AlertResponse?> ResolveAlertAsync(string tenantId, int alertId, ResolveAlertRequest request, CancellationToken cancellationToken = default)
    {
        var alert = await GetAlertDetailAsync(tenantId, alertId, cancellationToken);
        if (alert is null || alert.Status is not ("Active" or "Acknowledged")) return null;

        return alert with
        {
            Status = "Resolved",
            IsActive = false,
            ResolvedByEmail = request.ResolvedByEmail,
            ResolvedUtc = DateTime.UtcNow,
            ResolutionNotes = request.ResolutionNotes,
            UpdatedUtc = DateTime.UtcNow
        };
    }

    public async Task<AlertResponse?> ReopenAlertAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
    {
        var alert = await GetAlertDetailAsync(tenantId, alertId, cancellationToken);
        if (alert is null || alert.Status is not ("Resolved" or "Closed")) return null;

        return alert with
        {
            Status = "Active",
            IsActive = true,
            ResolvedByEmail = null,
            ResolvedUtc = null,
            ResolutionNotes = null,
            UpdatedUtc = DateTime.UtcNow
        };
    }

    public async Task<AlertResponse?> UpdateAlertOwnerAsync(string tenantId, int alertId, UpdateAlertOwnerRequest request, CancellationToken cancellationToken = default)
    {
        var alert = await GetAlertDetailAsync(tenantId, alertId, cancellationToken);
        return alert is null ? null : alert with { OwnerTeam = request.OwnerTeam, UpdatedUtc = DateTime.UtcNow };
    }

    public async Task<bool> CloseAlertAsync(string tenantId, int alertId, CancellationToken cancellationToken = default)
    {
        var alert = await GetAlertDetailAsync(tenantId, alertId, cancellationToken);
        return alert is not null;
    }

    // ── Reports / Settings ────────────────────────────────────────────────────

    public Task<IReadOnlyList<ReportSnapshotResponse>> GetReportsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ReportSnapshotResponse> items =
        [
            new("On-time departure", "96.4%", "+1.2% week over week"),
            new("Average dock turn", "44 min", "-3 min versus last week"),
            new("Asset utilization", "81%", "+4 points this month")
        ];
        return Task.FromResult(items);
    }

    public Task<IReadOnlyList<SettingsSectionResponse>> GetSettingsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SettingsSectionResponse> items =
        [
            new("Tenant Controls",
            [
                "Tenant header mapping: X-Tenant-Id",
                "Default operational region: Pacific",
                "Brand profile: Atlas Meridian Logistics"
            ]),
            new("Dispatch Rules",
            [
                "Auto-flag delays after 12 minutes",
                "Recompute dock assignment every 90 seconds",
                "Escalate missed departure after 2 failed retries"
            ])
        ];
        return Task.FromResult(items);
    }

    public async Task<OperationalOverviewResponse> GetOverviewAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var summary  = await GetSummaryAsync(tenantId, cancellationToken);
        var yards    = await GetYardsAsync(tenantId, cancellationToken);
        var alerts   = await GetAlertsAsync(tenantId, cancellationToken);
        var reports  = await GetReportsAsync(tenantId, cancellationToken);
        var settings = await GetSettingsAsync(tenantId, cancellationToken);

        return new OperationalOverviewResponse(
            summary,
            AllFleet(),
            yards,
            AllRoutes(),
            alerts,
            reports,
            settings);
    }

    public Task<IReadOnlyList<TenantSettingResponse>> GetTenantSettingsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TenantSettingResponse> settings =
        [
            new("Brand profile",                "Atlas Meridian Logistics"),
            new("Default operational region",   "Pacific"),
            new("Tenant header mapping",        "X-Tenant-Id"),
            new("Auto-flag delays after",       "12 minutes"),
            new("Recompute dock assignment every", "90 seconds"),
            new("Escalate missed departure after", "2 failed retries")
        ];
        return Task.FromResult(settings);
    }

    public Task<TenantSettingResponse> UpdateTenantSettingAsync(string tenantId, string key, string value, CancellationToken cancellationToken = default)
        => Task.FromResult(new TenantSettingResponse(key, value));

    public Task<ReportSummaryResponse> GetReportSummaryAsync(string tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ReportSummaryResponse(
            From: from, To: to,
            TotalAlerts: 14, CriticalAlerts: 3, WarningAlerts: 8, InfoAlerts: 3,
            TotalVehicles: 5, InTransitVehicles: 2, AtDockVehicles: 1, AwaitingDispatchVehicles: 1, DelayedVehicles: 1,
            TotalRoutes: 5, ActiveRoutes: 4, OnScheduleRoutes: 3,
            TotalYards: 3, AvgYardOccupancyPercent: 72));
    }

    private IReadOnlyList<UserResponse> AllUsers() =>
    [
        new(1, "morgan.ellis@atlasmeridian.example", "Morgan Ellis", "Tenant Admin", true,  DateTime.UtcNow.AddMonths(-6)),
        new(2, "priya.shah@atlasmeridian.example",   "Priya Shah",   "Dispatcher",   true,  DateTime.UtcNow.AddMonths(-4)),
        new(3, "darius.cole@atlasmeridian.example",  "Darius Cole",  "Yard Manager", true,  DateTime.UtcNow.AddMonths(-2))
    ];

    public Task<PagedResponse<UserResponse>> GetUsersAsync(string tenantId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var all = AllUsers();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResponse<UserResponse>(items, all.Count, page, pageSize));
    }

    public Task<UserResponse> CreateUserAsync(string tenantId, CreateUserRequest request, string passwordHash, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new UserResponse(AllUsers().Count + 1, request.Email, request.DisplayName, request.Role, true, DateTime.UtcNow));
    }

    public Task<UserResponse?> UpdateUserAsync(string tenantId, int userId, UpdateUserRequest request, string? newPasswordHash, CancellationToken cancellationToken = default)
    {
        var user = AllUsers().FirstOrDefault(u => u.UserId == userId);
        return user is null ? null : user with { DisplayName = request.DisplayName, Role = request.Role };
    }

    public Task<bool> DeactivateUserAsync(string tenantId, int userId, CancellationToken cancellationToken = default)
        => Task.FromResult(AllUsers().Any(u => u.UserId == userId));

    public Task<UserResponse?> ReactivateUserAsync(string tenantId, int userId, CancellationToken cancellationToken = default)
    {
        var user = AllUsers().FirstOrDefault(u => u.UserId == userId);
        return Task.FromResult(user is null ? null : user with { IsActive = true });
    }
}
