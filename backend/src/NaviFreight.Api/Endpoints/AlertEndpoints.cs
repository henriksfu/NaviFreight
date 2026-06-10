using NaviFreight.Api.Extensions;
using NaviFreight.Api.Models;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class AlertEndpoints
{
    private static readonly string[] ValidSeverities = ["Critical", "Warning", "Info"];
    private static readonly string[] ValidStatuses   = ["Active", "Acknowledged", "Resolved", "Closed"];

    public static IEndpointRouteBuilder MapAlertEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/alerts")
            .WithTags("Alerts")
            .RequireAuthorization();

        // ── List (with optional status/severity filter) ────────────────────
        group.MapGet("/", async (
            HttpContext context,
            IOperationsDataService operations,
            string? status,
            string? severity,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var tenant = context.GetTenantContext();

            if (status is not null && !ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["status"] = [$"Status must be one of: {string.Join(", ", ValidStatuses)}."]
                });

            if (severity is not null && !ValidSeverities.Contains(severity, StringComparer.OrdinalIgnoreCase))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["severity"] = [$"Severity must be one of: {string.Join(", ", ValidSeverities)}."]
                });

            return Results.Ok(await operations.GetAlertListAsync(tenant.TenantId, status, severity, page, pageSize, ct));
        });

        // ── Detail ────────────────────────────────────────────────────────
        group.MapGet("/{alertId:int}", async (HttpContext context, int alertId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var alert = await operations.GetAlertDetailAsync(tenant.TenantId, alertId, ct);
            return alert is null ? Results.NotFound() : Results.Ok(alert);
        });

        // ── Create ────────────────────────────────────────────────────────
        group.MapPost("/", async (HttpContext context, CreateAlertRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateCreate(request);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var created = await operations.CreateAlertAsync(tenant.TenantId, request, ct);
            return Results.Created($"/api/alerts/{created.AlertEventId}", created);
        })
        .RequireAuthorization("DispatchWrite");

        // ── Acknowledge ───────────────────────────────────────────────────
        group.MapPost("/{alertId:int}/acknowledge", async (HttpContext context, int alertId, AcknowledgeAlertRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            if (string.IsNullOrWhiteSpace(request.AcknowledgedByEmail))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["acknowledgedByEmail"] = ["Acknowledged-by email is required."]
                });

            var alert = await operations.AcknowledgeAlertAsync(tenant.TenantId, alertId, request, ct);
            return alert is null
                ? Results.UnprocessableEntity(new { error = "Alert not found or is not in Active status." })
                : Results.Ok(alert);
        });

        // ── Resolve ───────────────────────────────────────────────────────
        group.MapPost("/{alertId:int}/resolve", async (HttpContext context, int alertId, ResolveAlertRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            if (string.IsNullOrWhiteSpace(request.ResolvedByEmail))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["resolvedByEmail"] = ["Resolved-by email is required."]
                });

            var alert = await operations.ResolveAlertAsync(tenant.TenantId, alertId, request, ct);
            return alert is null
                ? Results.UnprocessableEntity(new { error = "Alert not found or cannot be resolved from its current status." })
                : Results.Ok(alert);
        })
        .RequireAuthorization("DispatchWrite");

        // ── Reopen ────────────────────────────────────────────────────────
        group.MapPost("/{alertId:int}/reopen", async (HttpContext context, int alertId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var alert = await operations.ReopenAlertAsync(tenant.TenantId, alertId, ct);
            return alert is null
                ? Results.UnprocessableEntity(new { error = "Alert not found or is not in Resolved or Closed status." })
                : Results.Ok(alert);
        })
        .RequireAuthorization("DispatchWrite");

        // ── Reassign owner ────────────────────────────────────────────────
        group.MapPut("/{alertId:int}/owner", async (HttpContext context, int alertId, UpdateAlertOwnerRequest request, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            if (string.IsNullOrWhiteSpace(request.OwnerTeam))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["ownerTeam"] = ["Owner team is required."]
                });

            var alert = await operations.UpdateAlertOwnerAsync(tenant.TenantId, alertId, request, ct);
            return alert is null ? Results.NotFound() : Results.Ok(alert);
        })
        .RequireAuthorization("DispatchWrite");

        // ── Close (hard close — TenantAdmin only) ─────────────────────────
        group.MapDelete("/{alertId:int}", async (HttpContext context, int alertId, IOperationsDataService operations, CancellationToken ct) =>
        {
            var tenant = context.GetTenantContext();
            var closed = await operations.CloseAlertAsync(tenant.TenantId, alertId, ct);
            return closed ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("TenantAdmin");

        return app;
    }

    private static Dictionary<string, string[]> ValidateCreate(CreateAlertRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Severity))
            errors["severity"] = ["Severity is required."];
        else if (!ValidSeverities.Contains(request.Severity, StringComparer.OrdinalIgnoreCase))
            errors["severity"] = [$"Severity must be one of: {string.Join(", ", ValidSeverities)}."];

        if (string.IsNullOrWhiteSpace(request.Title))
            errors["title"] = ["Title is required."];

        if (string.IsNullOrWhiteSpace(request.Description))
            errors["description"] = ["Description is required."];

        if (string.IsNullOrWhiteSpace(request.OwnerTeam))
            errors["ownerTeam"] = ["Owner team is required."];

        return errors;
    }
}
