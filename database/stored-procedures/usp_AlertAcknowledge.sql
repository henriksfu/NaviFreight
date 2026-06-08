CREATE OR ALTER PROCEDURE dbo.usp_AlertAcknowledge
    @TenantId           NVARCHAR(50),
    @AlertEventId       INT,
    @AcknowledgedByEmail NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AlertEvents
    SET Status              = 'Acknowledged',
        AcknowledgedByEmail = @AcknowledgedByEmail,
        AcknowledgedUtc     = SYSUTCDATETIME(),
        UpdatedUtc          = SYSUTCDATETIME()
    WHERE AlertEventId = @AlertEventId
      AND TenantId     = @TenantId
      AND Status       = 'Active';   -- only active alerts can be acknowledged

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
