namespace NaviFreight.Api.Models;

public sealed record CreateVehicleRequest(
    string VehicleId,
    string Status,
    int? CurrentYardId,
    DateTime? EtaUtc,
    int UtilizationPercent);

public sealed record UpdateVehicleRequest(
    string Status,
    int? CurrentYardId,
    DateTime? EtaUtc,
    int UtilizationPercent);

public sealed record AssignDriverRequest(int DriverId);

public sealed record CreateDriverRequest(
    string FullName,
    string LicenseNumber,
    string AvailabilityStatus);

public sealed record UpdateDriverRequest(
    string FullName,
    string LicenseNumber,
    string AvailabilityStatus);
