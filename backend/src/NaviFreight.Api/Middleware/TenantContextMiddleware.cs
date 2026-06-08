using System.Security.Claims;
using Microsoft.Extensions.Options;
using NaviFreight.Api.Configuration;
using NaviFreight.Api.Models;

namespace NaviFreight.Api.Middleware;

public sealed class TenantContextMiddleware(
    RequestDelegate next,
    IOptions<TenantOptions> tenantOptions)
{
    public async Task Invoke(HttpContext context)
    {
        var options = tenantOptions.Value;

        // Primary: explicit header (allows clients to scope the request).
        var tenantId = context.Request.Headers[options.HeaderName].FirstOrDefault();

        // Fallback: tenant_id embedded in the validated JWT by the auth middleware.
        if (string.IsNullOrWhiteSpace(tenantId))
            tenantId = context.User?.FindFirstValue("tenant_id");

        // Last resort: configured default.
        if (string.IsNullOrWhiteSpace(tenantId))
            tenantId = options.DefaultTenantId;

        context.Items[nameof(TenantContext)] = new TenantContext(tenantId);

        await next(context);
    }
}
