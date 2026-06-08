CREATE OR ALTER PROCEDURE dbo.usp_DriverUpdate
    @TenantId NVARCHAR(50),
    @DriverId INT,
    @FullName NVARCHAR(150),
    @LicenseNumber NVARCHAR(50),
    @AvailabilityStatus NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Drivers
    SET FullName = @FullName,
        LicenseNumber = @LicenseNumber,
        AvailabilityStatus = @AvailabilityStatus
    WHERE TenantId = @TenantId
      AND DriverId = @DriverId;

    IF @@ROWCOUNT = 0
    BEGIN
        RETURN;
    END;

    UPDATE dbo.Vehicles
    SET DriverName = @FullName,
        LastUpdatedUtc = SYSUTCDATETIME()
    WHERE TenantId = @TenantId
      AND DriverId = @DriverId;

    EXEC dbo.usp_DriverDetail @TenantId = @TenantId, @DriverId = @DriverId;
END;
