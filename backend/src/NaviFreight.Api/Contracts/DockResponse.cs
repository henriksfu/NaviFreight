namespace NaviFreight.Api.Contracts;

public sealed record DockResponse(
    int       DockId,
    int       YardId,
    string    DockCode,
    string    Status,
    string?   OccupyingVehicleId,
    string?   Notes,
    DateTime  CreatedUtc,
    DateTime? UpdatedUtc);
