using NaviFreight.Api.Extensions;
using NaviFreight.Api.Models;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class RouteEndpoints
{
    public static IEndpointRouteBuilder MapRouteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/routes")
            .WithTags("Routes")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();

            return Results.Ok(new
            {
                tenantId = tenant.TenantId,
                items = await operations.GetRoutesAsync(tenant.TenantId, cancellationToken)
            });
        });

        group.MapGet("/{routeCode}", async (HttpContext context, string routeCode, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var route = await operations.GetRouteByCodeAsync(tenant.TenantId, routeCode, cancellationToken);
            return route is null ? Results.NotFound() : Results.Ok(route);
        });

        group.MapPost("/", async (HttpContext context, CreateRouteRequest request, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();

            var errors = ValidateCreateRoute(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var created = await operations.CreateRouteAsync(tenant.TenantId, request, cancellationToken);
            return Results.Created($"/api/routes/{created.RouteCode}", created);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapPut("/{routeCode}", async (HttpContext context, string routeCode, UpdateRouteRequest request, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateUpdateRoute(request);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }
            var updated = await operations.UpdateRouteAsync(tenant.TenantId, routeCode, request, cancellationToken);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapDelete("/{routeCode}", async (HttpContext context, string routeCode, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var deleted = await operations.DeleteRouteAsync(tenant.TenantId, routeCode, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("DispatchWrite");

        group.MapPost("/{routeCode}/assignments", async (HttpContext context, string routeCode, AssignVehiclesRequest request, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();

            if (request.VehicleIds is null || request.VehicleIds.Count == 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["vehicleIds"] = ["At least one vehicle is required."]
                });
            }

            var detail = await operations.AssignVehiclesAsync(tenant.TenantId, routeCode, request.VehicleIds, cancellationToken);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapDelete("/{routeCode}/assignments/{vehicleId}", async (HttpContext context, string routeCode, string vehicleId, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var detail = await operations.UnassignVehicleAsync(tenant.TenantId, routeCode, vehicleId, cancellationToken);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .RequireAuthorization("DispatchWrite");

        return app;
    }

    private static Dictionary<string, string[]> ValidateCreateRoute(CreateRouteRequest request)
    {
        var errors = ValidateUpdateCore(request.DestinationName, request.Status, request.CompletionPercent);

        if (string.IsNullOrWhiteSpace(request.RouteCode))
        {
            errors["routeCode"] = ["Route code is required."];
        }

        if (request.OriginYardId <= 0)
        {
            errors["originYardId"] = ["Origin yard is required."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateUpdateRoute(UpdateRouteRequest request)
        => ValidateUpdateCore(request.DestinationName, request.Status, request.CompletionPercent);

    private static Dictionary<string, string[]> ValidateUpdateCore(string destinationName, string status, int completionPercent)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(destinationName))
        {
            errors["destinationName"] = ["Destination name is required."];
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            errors["status"] = ["Status is required."];
        }

        if (completionPercent is < 0 or > 100)
        {
            errors["completionPercent"] = ["Completion percent must be between 0 and 100."];
        }

        return errors;
    }
}
