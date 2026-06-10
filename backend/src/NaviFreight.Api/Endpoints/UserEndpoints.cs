using Microsoft.AspNetCore.Mvc;
using NaviFreight.Api.Extensions;
using NaviFreight.Api.Models;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .RequireAuthorization("TenantAdmin");

        group.MapGet("/", async (
            IOperationsDataService svc,
            HttpContext ctx,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var tenantId = ctx.GetTenantContext().TenantId;
            return Results.Ok(await svc.GetUsersAsync(tenantId, page, pageSize, ct));
        });

        group.MapPost("/", async (
            [FromBody] CreateUserRequest req,
            IOperationsDataService svc,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var errors = ValidateCreate(req);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var tenantId = ctx.GetTenantContext().TenantId;
            var hash = PasswordHasher.Hash(req.Password);
            var created = await svc.CreateUserAsync(tenantId, req, hash, ct);
            return Results.Created($"/api/users/{created.UserId}", created);
        });

        group.MapPut("/{userId:int}", async (
            int userId,
            [FromBody] UpdateUserRequest req,
            IOperationsDataService svc,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var errors = ValidateUpdate(req);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var tenantId = ctx.GetTenantContext().TenantId;
            var newHash = string.IsNullOrWhiteSpace(req.NewPassword) ? null : PasswordHasher.Hash(req.NewPassword);
            var updated = await svc.UpdateUserAsync(tenantId, userId, req, newHash, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        group.MapDelete("/{userId:int}", async (
            int userId,
            IOperationsDataService svc,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var tenantId = ctx.GetTenantContext().TenantId;
            var ok = await svc.DeactivateUserAsync(tenantId, userId, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        group.MapPost("/{userId:int}/reactivate", async (
            int userId,
            IOperationsDataService svc,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var tenantId = ctx.GetTenantContext().TenantId;
            var result = await svc.ReactivateUserAsync(tenantId, userId, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        return app;
    }

    private static Dictionary<string, string[]> ValidateCreate(CreateUserRequest req)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(req.Email))        errors["email"]       = ["Email is required."];
        if (string.IsNullOrWhiteSpace(req.DisplayName))  errors["displayName"] = ["Display name is required."];
        if (string.IsNullOrWhiteSpace(req.Password))     errors["password"]    = ["Password is required."];
        if (!IsValidRole(req.Role))                      errors["role"]        = ["Role must be Tenant Admin, Dispatcher, or Yard Manager."];
        return errors;
    }

    private static Dictionary<string, string[]> ValidateUpdate(UpdateUserRequest req)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(req.DisplayName)) errors["displayName"] = ["Display name is required."];
        if (!IsValidRole(req.Role))                     errors["role"]        = ["Role must be Tenant Admin, Dispatcher, or Yard Manager."];
        return errors;
    }

    private static bool IsValidRole(string role) =>
        role is "Tenant Admin" or "Dispatcher" or "Yard Manager";
}
