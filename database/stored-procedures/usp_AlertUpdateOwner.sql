CREATE OR ALTER PROCEDURE dbo.usp_AlertUpdateOwner
    @TenantId     NVARCHAR(50),
    @AlertEventId INT,
    @OwnerTeam    NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AlertEvents
    SET OwnerTeam  = @OwnerTeam,
        UpdatedUtc = SYSUTCDATETIME()
    WHERE AlertEventId = @AlertEventId
      AND TenantId     = @TenantId;

    IF @@ROWCOUNT = 0
        RETURN;

    SELECT
        AlertEventId,
        Severity,
        Title,
        Description,
        OwnerTeam,
        Status,
        IsActive,
        CreatedUtc,
        AcknowledgedByEmail,
        AcknowledgedUtc,
        ResolvedByEmail,
        ResolvedUtc,
        ResolutionNotes,
        UpdatedUtc
    FROM dbo.AlertEvents
    WHERE AlertEventId = @AlertEventId;
END;
