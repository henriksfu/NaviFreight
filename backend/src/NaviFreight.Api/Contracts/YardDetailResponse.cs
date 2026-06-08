namespace NaviFreight.Api.Contracts;

public sealed record YardDetailResponse(
    YardResponse              Yard,
    IReadOnlyList<DockResponse> Docks);
