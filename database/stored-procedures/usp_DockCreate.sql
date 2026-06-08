CREATE OR ALTER PROCEDURE dbo.usp_DockCreate
    @TenantId NVARCHAR(50),
    @YardId   INT,
    @DockCode NVARCHAR(50),
    @Status   NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Verify yard belongs to this tenant
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Yards
        WHERE YardId = @YardId AND TenantId = @TenantId AND IsActive = 1
    )
    BEGIN
        SELECT CAST(NULL AS INT) AS DockId WHERE 1 = 0;
        RETURN;
    END;

    INSERT INTO dbo.Docks (YardId, DockCode, Status)
    VALUES (@YardId, @DockCode, @Status);

    DECLARE @NewDockId INT = SCOPE_IDENTITY();

    SELECT
        d.DockId,
        d.YardId,
        d.DockCode,
        d.Status,
        d.OccupyingVehicleId,
        d.Notes,
        d.CreatedUtc,
        d.UpdatedUtc
    FROM dbo.Docks d
    WHERE d.DockId = @NewDockId;
END;
