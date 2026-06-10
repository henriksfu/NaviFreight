using Microsoft.AspNetCore.Mvc;
using NaviFreight.Api.Extensions;
using NaviFreight.Api.Models;
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

        group.MapGet("/tenant", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var settings = await operations.GetTenantSettingsAsync(tenant.TenantId, cancellationToken);
            return Results.Ok(settings);
        });

        group.MapPut("/tenant/{key}", async (
            string key,
            [FromBody] UpdateSettingRequest req,
            HttpContext context,
            IOperationsDataService operations,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(req.Value))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["value"] = ["Value is required."] });

            var tenant = context.GetTenantContext();
            var updated = await operations.UpdateTenantSettingAsync(tenant.TenantId, key, req.Value, cancellationToken);
            return Results.Ok(updated);
        });

        return app;
    }
}
