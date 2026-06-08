namespace NaviFreight.Api.Contracts;

public sealed record SettingsSectionResponse(
    string Title,
    IReadOnlyList<string> Items);
