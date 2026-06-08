CREATE OR ALTER PROCEDURE dbo.usp_DockUpdate
    @TenantId NVARCHAR(50),
    @YardId   INT,
    @DockId   INT,
    @DockCode NVARCHAR(50),
    @Status   NVARCHAR(50),
    @Notes    NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Docks
    SET DockCode   = @DockCode,
        Status     = @Status,
        Notes      = @Notes,
        UpdatedUtc = SYSUTCDATETIME()
    FROM dbo.Docks d
    INNER JOIN dbo.Yards y ON y.YardId = d.YardId
    WHERE d.DockId   = @DockId
      AND d.YardId   = @YardId
      AND y.TenantId = @TenantId;

    IF @@ROWCOUNT = 0
        RETURN;

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
    WHERE d.DockId = @DockId;
END;
