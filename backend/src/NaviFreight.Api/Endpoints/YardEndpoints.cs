using NaviFreight.Api.Extensions;
using NaviFreight.Api.Models;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class YardEndpoints
{
    private static readonly string[] ValidDockStatuses = ["Available", "Occupied", "Maintenance", "Offline"];

    public static IEndpointRouteBuilder MapYardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/yards")
            .WithTags("Yards")
            .RequireAuthorization();

        // ── Snapshot (used by dashboard overview) ─────────────────────────────
        group.MapGet("/snapshots", async (HttpContext context, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            return Results.Ok(new
            {
                tenantId = tenant.TenantId,
                items = await operations.GetYardsAsync(tenant.TenantId, ct)
            });
        });

        // ── Yards CRUD ────────────────────────────────────────────────────────
        group.MapGet("/", async (HttpContext context, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            return Results.Ok(new
            {
                tenantId = tenant.TenantId,
                items = await operations.GetYardListAsync(tenant.TenantId, ct)
            });
        });

        group.MapGet("/{yardId:int}", async (HttpContext context, int yardId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var detail = await operations.GetYardDetailAsync(tenant.TenantId, yardId, ct);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        });

        group.MapPost("/", async (HttpContext context, CreateYardRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateYard(request.YardName, request.Capacity);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var created = await operations.CreateYardAsync(tenant.TenantId, request, ct);
            return Results.Created($"/api/yards/{created.Yard.YardId}", created);
        })
        .RequireAuthorization("TenantAdmin");

        group.MapPut("/{yardId:int}", async (HttpContext context, int yardId, UpdateYardRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateYard(request.YardName, request.Capacity);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var updated = await operations.UpdateYardAsync(tenant.TenantId, yardId, request, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .RequireAuthorization("TenantAdmin");

        group.MapDelete("/{yardId:int}", async (HttpContext context, int yardId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var deleted = await operations.DeleteYardAsync(tenant.TenantId, yardId, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("TenantAdmin");

        group.MapPatch("/{yardId:int}/operational", async (HttpContext context, int yardId, YardOperationalUpdateRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateOperationalUpdate(request);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var updated = await operations.UpdateYardOperationalAsync(tenant.TenantId, yardId, request, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .RequireAuthorization("DispatchWrite");

        // ── Docks CRUD ────────────────────────────────────────────────────────
        group.MapGet("/{yardId:int}/docks", async (HttpContext context, int yardId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            return Results.Ok(new
            {
                yardId,
                items = await operations.GetDocksAsync(tenant.TenantId, yardId, ct)
            });
        });

        group.MapGet("/{yardId:int}/docks/{dockId:int}", async (HttpContext context, int yardId, int dockId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var dock = await operations.GetDockDetailAsync(tenant.TenantId, yardId, dockId, ct);
            return dock is null ? Results.NotFound() : Results.Ok(dock);
        });

        group.MapPost("/{yardId:int}/docks", async (HttpContext context, int yardId, CreateDockRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateDock(request.DockCode, request.Status);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var created = await operations.CreateDockAsync(tenant.TenantId, yardId, request, ct);
            return created is null
                ? Results.NotFound()
                : Results.Created($"/api/yards/{yardId}/docks/{created.DockId}", created);
        })
        .RequireAuthorization("TenantAdmin");

        group.MapPut("/{yardId:int}/docks/{dockId:int}", async (HttpContext context, int yardId, int dockId, UpdateDockRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateDock(request.DockCode, request.Status);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var updated = await operations.UpdateDockAsync(tenant.TenantId, yardId, dockId, request, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapDelete("/{yardId:int}/docks/{dockId:int}", async (HttpContext context, int yardId, int dockId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var deleted = await operations.DeleteDockAsync(tenant.TenantId, yardId, dockId, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("TenantAdmin");

        // ── Dock operational: assign / release vehicle ─────────────────────
        group.MapPost("/{yardId:int}/docks/{dockId:int}/vehicle", async (HttpContext context, int yardId, int dockId, AssignVehicleToDockRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            if (string.IsNullOrWhiteSpace(request.VehicleId))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["vehicleId"] = ["Vehicle id is required."] });

            var dock = await operations.AssignVehicleToDockAsync(tenant.TenantId, yardId, dockId, request.VehicleId, ct);
            return dock is null ? Results.NotFound() : Results.Ok(dock);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapDelete("/{yardId:int}/docks/{dockId:int}/vehicle", async (HttpContext context, int yardId, int dockId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var dock = await operations.ReleaseDockAsync(tenant.TenantId, yardId, dockId, ct);
            return dock is null ? Results.NotFound() : Results.Ok(dock);
        })
        .RequireAuthorization("DispatchWrite");

        return app;
    }

    private static Dictionary<string, string[]> ValidateYard(string yardName, int capacity)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(yardName))
            errors["yardName"] = ["Yard name is required."];

        if (capacity <= 0)
            errors["capacity"] = ["Capacity must be greater than zero."];

        return errors;
    }

    private static Dictionary<string, string[]> ValidateOperationalUpdate(YardOperationalUpdateRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.OccupiedSlots < 0)
            errors["occupiedSlots"] = ["Occupied slots must be zero or greater."];

        if (request.InboundQueue < 0)
            errors["inboundQueue"] = ["Inbound queue must be zero or greater."];

        if (request.AverageTurnMinutes < 0)
            errors["averageTurnMinutes"] = ["Average turn minutes must be zero or greater."];

        return errors;
    }

    private static Dictionary<string, string[]> ValidateDock(string dockCode, string status)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(dockCode))
            errors["dockCode"] = ["Dock code is required."];

        if (string.IsNullOrWhiteSpace(status))
        {
            errors["status"] = ["Status is required."];
        }
        else if (!ValidDockStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
        {
            errors["status"] = [$"Status must be one of: {string.Join(", ", ValidDockStatuses)}."];
        }

        return errors;
    }
}
