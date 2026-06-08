namespace NaviFreight.Api.Contracts;

public sealed record RouteDispatchAssignmentResponse(
    string VehicleId,
    string DriverName,
    string Status,
    string CurrentYard,
    DateTime? EtaUtc);
