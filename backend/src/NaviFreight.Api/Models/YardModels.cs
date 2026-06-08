namespace NaviFreight.Api.Models;

public sealed record CreateYardRequest(
    string YardName,
    int    Capacity);

public sealed record UpdateYardRequest(
    string YardName,
    int    Capacity);

public sealed record YardOperationalUpdateRequest(
    int OccupiedSlots,
    int InboundQueue,
    int AverageTurnMinutes);

public sealed record CreateDockRequest(
    string  DockCode,
    string  Status);

public sealed record UpdateDockRequest(
    string  DockCode,
    string  Status,
    string? Notes);

public sealed record AssignVehicleToDockRequest(
    string VehicleId);
