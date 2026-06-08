namespace NaviFreight.Api.Models;

public sealed record CreateRouteRequest(
    string RouteCode,
    int OriginYardId,
    string DestinationName,
    string Status,
    DateTime NextDepartureUtc,
    int CompletionPercent);

public sealed record UpdateRouteRequest(
    string DestinationName,
    string Status,
    DateTime NextDepartureUtc,
    int CompletionPercent);

public sealed record AssignVehiclesRequest(
    IReadOnlyList<string> VehicleIds);
