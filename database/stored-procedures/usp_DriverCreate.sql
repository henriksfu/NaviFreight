CREATE OR ALTER PROCEDURE dbo.usp_DriverCreate
    @TenantId NVARCHAR(50),
    @FullName NVARCHAR(150),
    @LicenseNumber NVARCHAR(50),
    @AvailabilityStatus NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Drivers (
        TenantId,
        FullName,
        LicenseNumber,
        AvailabilityStatus)
    VALUES (
        @TenantId,
        @FullName,
        @LicenseNumber,
        @AvailabilityStatus);

    DECLARE @DriverId INT = SCOPE_IDENTITY();

    EXEC dbo.usp_DriverDetail @TenantId = @TenantId, @DriverId = @DriverId;
END;
