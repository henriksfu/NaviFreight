using NaviFreight.Api.Extensions;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .RequireAuthorization();

        group.MapGet("/snapshots", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();

            return Results.Ok(new
            {
                tenantId = tenant.TenantId,
                items = await operations.GetReportsAsync(tenant.TenantId, cancellationToken)
            });
        });

        return app;
    }
}
