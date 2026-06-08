CREATE OR ALTER PROCEDURE dbo.usp_AlertDetail
    @TenantId    NVARCHAR(50),
    @AlertEventId INT
AS
BEGIN
    SET NOCOUNT ON;

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
    WHERE AlertEventId = @AlertEventId
      AND TenantId     = @TenantId;
END;
