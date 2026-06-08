namespace NaviFreight.Api.Models;

public sealed record CreateAlertRequest(
    string Severity,
    string Title,
    string Description,
    string OwnerTeam);

public sealed record AcknowledgeAlertRequest(
    string AcknowledgedByEmail);

public sealed record ResolveAlertRequest(
    string  ResolvedByEmail,
    string? ResolutionNotes);

public sealed record UpdateAlertOwnerRequest(
    string OwnerTeam);
