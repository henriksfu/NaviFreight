CREATE OR ALTER PROCEDURE dbo.usp_FleetVehicleUpdate
    @TenantId NVARCHAR(50),
    @VehicleId NVARCHAR(50),
    @Status NVARCHAR(50),
    @CurrentYardId INT = NULL,
    @EtaUtc DATETIME2 = NULL,
    @UtilizationPercent INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Vehicles
    SET Status = @Status,
        CurrentYardId = @CurrentYardId,
        EtaUtc = @EtaUtc,
        UtilizationPercent = @UtilizationPercent,
        LastUpdatedUtc = SYSUTCDATETIME()
    WHERE TenantId = @TenantId
      AND VehicleId = @VehicleId;

    IF @@ROWCOUNT = 0
    BEGIN
        RETURN;
    END;

    EXEC dbo.usp_FleetVehicleDetail @TenantId = @TenantId, @VehicleId = @VehicleId;
END;
