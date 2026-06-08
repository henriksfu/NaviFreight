namespace NaviFreight.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = "navifreight-api";
    public string Audience { get; init; } = "navifreight-app";
    public int ExpiryMinutes { get; init; } = 480;
}
