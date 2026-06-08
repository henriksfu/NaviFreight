using NaviFreight.Api.Models;

namespace NaviFreight.Api.Extensions;

public static class HttpContextExtensions
{
    public static TenantContext GetTenantContext(this HttpContext context)
    {
        return context.Items[nameof(TenantContext)] as TenantContext
            ?? new TenantContext("tenant-demo");
    }
}
