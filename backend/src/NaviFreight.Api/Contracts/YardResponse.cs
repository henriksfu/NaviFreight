namespace NaviFreight.Api.Contracts;

public sealed record YardResponse(
    int      YardId,
    string   YardName,
    int      Capacity,
    int      OccupiedSlots,
    int      InboundQueue,
    int      AverageTurnMinutes,
    int      TotalDocks,
    int      AvailableDocks,
    bool     IsActive,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc);
