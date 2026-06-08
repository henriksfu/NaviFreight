namespace NaviFreight.Api.Contracts;

public sealed record DriverResponse(
    int DriverId,
    string FullName,
    string LicenseNumber,
    string AvailabilityStatus,
    string? AssignedVehicleId);
