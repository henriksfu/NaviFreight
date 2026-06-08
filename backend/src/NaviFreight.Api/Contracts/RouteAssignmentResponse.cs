namespace NaviFreight.Api.Contracts;

public sealed record RouteAssignmentResponse(
    string RouteCode,
    string Origin,
    string Destination,
    string Status,
    int AssignedVehicles,
    DateTime NextDepartureUtc,
    int CompletionPercent);
