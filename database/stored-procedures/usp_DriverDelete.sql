CREATE OR ALTER PROCEDURE dbo.usp_DriverDelete
    @TenantId NVARCHAR(50),
    @DriverId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Vehicles
    SET DriverId = NULL,
        DriverName = 'Unassigned',
        LastUpdatedUtc = SYSUTCDATETIME()
    WHERE TenantId = @TenantId
      AND DriverId = @DriverId;

    DELETE FROM dbo.Drivers
    WHERE TenantId = @TenantId
      AND DriverId = @DriverId;
END;
