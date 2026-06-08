namespace NaviFreight.Api.Contracts;

public sealed record RouteDetailResponse(
    RouteAssignmentResponse Route,
    IReadOnlyList<RouteDispatchAssignmentResponse> AssignedVehicles);
