namespace NaviFreight.Api.Configuration;

public sealed class DataAccessOptions
{
    public const string SectionName = "DataAccess";

    public string Provider { get; init; } = "InMemory";
}
