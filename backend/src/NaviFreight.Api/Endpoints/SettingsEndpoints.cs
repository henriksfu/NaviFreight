using NaviFreight.Api.Extensions;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings")
            .WithTags("Settings")
            .RequireAuthorization("TenantAdmin");

        group.MapGet("/", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();

            return Results.Ok(new
            {
                tenantId = tenant.TenantId,
                items = await operations.GetSettingsAsync(tenant.TenantId, cancellationToken)
            });
        });

        return app;
    }
}
