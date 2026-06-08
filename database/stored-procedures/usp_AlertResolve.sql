CREATE OR ALTER PROCEDURE dbo.usp_AlertResolve
    @TenantId       NVARCHAR(50),
    @AlertEventId   INT,
    @ResolvedByEmail NVARCHAR(200),
    @ResolutionNotes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AlertEvents
    SET Status          = 'Resolved',
        IsActive        = 0,
        ResolvedByEmail = @ResolvedByEmail,
        ResolvedUtc     = SYSUTCDATETIME(),
        ResolutionNotes = @ResolutionNotes,
        UpdatedUtc      = SYSUTCDATETIME()
    WHERE AlertEventId  = @AlertEventId
      AND TenantId      = @TenantId
      AND Status        IN ('Active', 'Acknowledged');  -- both statuses can be resolved

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
