CREATE OR ALTER PROCEDURE dbo.usp_AlertReopen
    @TenantId     NVARCHAR(50),
    @AlertEventId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AlertEvents
    SET Status          = 'Active',
        IsActive        = 1,
        ResolvedByEmail = NULL,
        ResolvedUtc     = NULL,
        ResolutionNotes = NULL,
        UpdatedUtc      = SYSUTCDATETIME()
    WHERE AlertEventId  = @AlertEventId
      AND TenantId      = @TenantId
      AND Status        IN ('Resolved', 'Closed');  -- only closed/resolved alerts can be reopened

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
