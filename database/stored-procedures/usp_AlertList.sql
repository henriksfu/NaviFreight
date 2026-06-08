CREATE OR ALTER PROCEDURE dbo.usp_AlertList
    @TenantId NVARCHAR(50),
    @Status   NVARCHAR(20) = NULL,   -- NULL = all; 'Active' | 'Acknowledged' | 'Resolved' | 'Closed'
    @Severity NVARCHAR(50) = NULL    -- NULL = all; 'Critical' | 'Warning' | 'Info'
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
    WHERE TenantId = @TenantId
      AND (@Status   IS NULL OR Status   = @Status)
      AND (@Severity IS NULL OR Severity = @Severity)
    ORDER BY
        CASE Severity
            WHEN 'Critical' THEN 1
            WHEN 'Warning'  THEN 2
            ELSE 3
        END,
        CreatedUtc DESC;
END;
