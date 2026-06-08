namespace NaviFreight.Api.Contracts;

public sealed record FleetVehicleDetailResponse(
    string VehicleId,
    int? DriverId,
    string DriverName,
    string Status,
    int? CurrentYardId,
    string CurrentYard,
    DateTime LastUpdatedUtc,
    DateTime? EtaUtc,
    int UtilizationPercent,
    string RouteCode,
    bool IsDispatchReady);
