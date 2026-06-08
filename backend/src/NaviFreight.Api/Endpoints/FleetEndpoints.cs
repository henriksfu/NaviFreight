using NaviFreight.Api.Extensions;
using NaviFreight.Api.Models;
using NaviFreight.Api.Services;

namespace NaviFreight.Api.Endpoints;

public static class FleetEndpoints
{
    public static IEndpointRouteBuilder MapFleetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/fleet")
            .WithTags("Fleet")
            .RequireAuthorization();

        group.MapGet("/vehicles", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();

            return Results.Ok(new
            {
                tenantId = tenant.TenantId,
                items = await operations.GetFleetAsync(tenant.TenantId, cancellationToken)
            });
        });

        group.MapGet("/vehicles/{vehicleId}", async (HttpContext context, string vehicleId, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var vehicle = await operations.GetVehicleByIdAsync(tenant.TenantId, vehicleId, cancellationToken);
            return vehicle is null ? Results.NotFound() : Results.Ok(vehicle);
        });

        group.MapPost("/vehicles", async (HttpContext context, CreateVehicleRequest request, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateVehicle(request.VehicleId, request.Status, request.UtilizationPercent, request.CurrentYardId, true);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var created = await operations.CreateVehicleAsync(tenant.TenantId, request, cancellationToken);
            return Results.Created($"/api/fleet/vehicles/{created.VehicleId}", created);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapPut("/vehicles/{vehicleId}", async (HttpContext context, string vehicleId, UpdateVehicleRequest request, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateVehicle(vehicleId, request.Status, request.UtilizationPercent, request.CurrentYardId, false);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var updated = await operations.UpdateVehicleAsync(tenant.TenantId, vehicleId, request, cancellationToken);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapDelete("/vehicles/{vehicleId}", async (HttpContext context, string vehicleId, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var deleted = await operations.DeleteVehicleAsync(tenant.TenantId, vehicleId, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("DispatchWrite");

        group.MapPost("/vehicles/{vehicleId}/driver", async (HttpContext context, string vehicleId, AssignDriverRequest request, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            if (request.DriverId <= 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["driverId"] = ["Driver id is required."] });
            }

            var detail = await operations.AssignDriverAsync(tenant.TenantId, vehicleId, request.DriverId, cancellationToken);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapDelete("/vehicles/{vehicleId}/driver", async (HttpContext context, string vehicleId, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var detail = await operations.UnassignDriverAsync(tenant.TenantId, vehicleId, cancellationToken);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapGet("/drivers", async (HttpContext context, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            return Results.Ok(new
            {
                tenantId = tenant.TenantId,
                items = await operations.GetDriversAsync(tenant.TenantId, cancellationToken)
            });
        });

        group.MapGet("/drivers/{driverId:int}", async (HttpContext context, int driverId, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var driver = await operations.GetDriverByIdAsync(tenant.TenantId, driverId, cancellationToken);
            return driver is null ? Results.NotFound() : Results.Ok(driver);
        });

        group.MapPost("/drivers", async (HttpContext context, CreateDriverRequest request, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateDriver(request.FullName, request.LicenseNumber, request.AvailabilityStatus);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var created = await operations.CreateDriverAsync(tenant.TenantId, request, cancellationToken);
            return Results.Created($"/api/fleet/drivers/{created.DriverId}", created);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapPut("/drivers/{driverId:int}", async (HttpContext context, int driverId, UpdateDriverRequest request, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var errors = ValidateDriver(request.FullName, request.LicenseNumber, request.AvailabilityStatus);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            var updated = await operations.UpdateDriverAsync(tenant.TenantId, driverId, request, cancellationToken);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .RequireAuthorization("DispatchWrite");

        group.MapDelete("/drivers/{driverId:int}", async (HttpContext context, int driverId, IOperationsDataService operations, CancellationToken cancellationToken) =>
        {
            var tenant = context.GetTenantContext();
            var deleted = await operations.DeleteDriverAsync(tenant.TenantId, driverId, cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization("DispatchWrite");

        return app;
    }

    private static Dictionary<string, string[]> ValidateVehicle(string vehicleId, string status, int utilizationPercent, int? currentYardId, bool requireVehicleId)
    {
        var errors = new Dictionary<string, string[]>();

        if (requireVehicleId && string.IsNullOrWhiteSpace(vehicleId))
        {
            errors["vehicleId"] = ["Vehicle id is required."];
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            errors["status"] = ["Status is required."];
        }

        if (utilizationPercent is < 0 or > 100)
        {
            errors["utilizationPercent"] = ["Utilization percent must be between 0 and 100."];
        }

        if (currentYardId is <= 0)
        {
            errors["currentYardId"] = ["Current yard id must be positive when provided."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateDriver(string fullName, string licenseNumber, string availabilityStatus)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            errors["fullName"] = ["Driver name is required."];
        }

        if (string.IsNullOrWhiteSpace(licenseNumber))
        {
            errors["licenseNumber"] = ["License number is required."];
        }

        if (string.IsNullOrWhiteSpace(availabilityStatus))
        {
            errors["availabilityStatus"] = ["Availability status is required."];
        }

        return errors;
    }
}
