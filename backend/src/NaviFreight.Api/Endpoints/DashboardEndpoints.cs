using NaviFreight.Api.Extensions;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        group.MapGet("/summary", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();

            return Results.Ok(await operations.GetSummaryAsync(tenant.TenantId, cancellationToken));
        });

        group.MapGet("/overview", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();

            return Results.Ok(await operations.GetOverviewAsync(tenant.TenantId, cancellationToken));
        });

        group.MapGet("/fleet", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            return Results.Ok(new
            {
                tenantId = tenant.TenantId,
                items = await operations.GetFleetAsync(tenant.TenantId, cancellationToken: cancellationToken)
            });
        });

        return app;
    }
}
