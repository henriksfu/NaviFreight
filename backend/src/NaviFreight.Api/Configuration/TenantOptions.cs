namespace NaviFreight.Api.Configuration;

public sealed class TenantOptions
{
    public const string SectionName = "Tenant";

    public string HeaderName { get; init; } = "X-Tenant-Id";
    public string DefaultTenantId { get; init; } = "tenant-demo";
}
