namespace NaviFreight.Api.Contracts;

public sealed record ReportSnapshotResponse(
    string ReportName,
    string Value,
    string ChangeLabel);
