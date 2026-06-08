namespace NaviFreight.Api.Contracts;

public sealed record FleetVehicleResponse(
    string VehicleId,
    string DriverName,
    string Status,
    string CurrentYard,
    DateTime LastUpdatedUtc,
    DateTime? EtaUtc,
    int UtilizationPercent,
    string RouteCode);
