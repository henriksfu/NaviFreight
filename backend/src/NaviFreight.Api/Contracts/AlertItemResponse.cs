namespace NaviFreight.Api.Contracts;

public sealed record AlertItemResponse(
    string Severity,
    string Title,
    string Description,
    string Owner);
