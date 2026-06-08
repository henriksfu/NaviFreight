using System.Security.Claims;
using NaviFreight.Api.Extensions;
using NaviFreight.Api.Models;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, HttpContext context, IAuthService authService) =>
        {
            var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            var result = await authService.LoginAsync(request, tenantId);

            return result is null
                ? Results.Problem("Invalid email or password.", statusCode: 401)
                : Results.Ok(result);
        })
        .AllowAnonymous();

        group.MapGet("/me", async (ClaimsPrincipal principal, HttpContext context, IAuthService authService) =>
        {
            // With MapInboundClaims = false, the JWT "email" claim is preserved as-is.
            var email = principal.FindFirstValue("email");
            var tenant = context.GetTenantContext();

            if (string.IsNullOrEmpty(email))
                return Results.Unauthorized();

            var profile = await authService.GetUserByEmailAsync(email, tenant.TenantId);
            return profile is null ? Results.Unauthorized() : Results.Ok(profile);
        })
        .RequireAuthorization();

        return app;
    }
}
