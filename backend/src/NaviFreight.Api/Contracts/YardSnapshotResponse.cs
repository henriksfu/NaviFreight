namespace NaviFreight.Api.Contracts;

public sealed record YardSnapshotResponse(
    string YardName,
    int OccupiedSlots,
    int TotalSlots,
    int InboundQueue,
    int AvailableDocks,
    int AverageTurnMinutes);
