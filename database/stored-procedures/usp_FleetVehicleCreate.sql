CREATE OR ALTER PROCEDURE dbo.usp_FleetVehicleCreate
    @TenantId NVARCHAR(50),
    @VehicleId NVARCHAR(50),
    @Status NVARCHAR(50),
    @CurrentYardId INT = NULL,
    @EtaUtc DATETIME2 = NULL,
    @UtilizationPercent INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Vehicles (
        VehicleId,
        TenantId,
        DriverName,
        Status,
        CurrentYardId,
        LastUpdatedUtc,
        EtaUtc,
        UtilizationPercent)
    VALUES (
        @VehicleId,
        @TenantId,
        'Unassigned',
        @Status,
        @CurrentYardId,
        SYSUTCDATETIME(),
        @EtaUtc,
        @UtilizationPercent);

    EXEC dbo.usp_FleetVehicleDetail @TenantId = @TenantId, @VehicleId = @VehicleId;
END;
