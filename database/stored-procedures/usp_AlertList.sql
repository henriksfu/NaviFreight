CREATE OR ALTER PROCEDURE dbo.usp_AlertList
    @TenantId NVARCHAR(50),
    @Status   NVARCHAR(20)  = NULL,
    @Severity NVARCHAR(50)  = NULL,
    @Page     INT           = 1,
    @PageSize INT           = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT
        COUNT(*) OVER()        AS TotalCount,
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
        CreatedUtc DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
