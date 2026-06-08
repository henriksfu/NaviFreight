namespace NaviFreight.Api.Contracts;

public sealed record AlertResponse(
    int       AlertEventId,
    string    Severity,
    string    Title,
    string    Description,
    string    OwnerTeam,
    string    Status,
    bool      IsActive,
    DateTime  CreatedUtc,
    string?   AcknowledgedByEmail,
    DateTime? AcknowledgedUtc,
    string?   ResolvedByEmail,
    DateTime? ResolvedUtc,
    string?   ResolutionNotes,
    DateTime? UpdatedUtc);
