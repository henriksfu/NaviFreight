CREATE OR ALTER PROCEDURE dbo.usp_AlertCreate
    @TenantId   NVARCHAR(50),
    @Severity   NVARCHAR(50),
    @Title      NVARCHAR(150),
    @Description NVARCHAR(400),
    @OwnerTeam  NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AlertEvents
        (TenantId, Severity, Title, Description, OwnerTeam, IsActive, Status)
    VALUES
        (@TenantId, @Severity, @Title, @Description, @OwnerTeam, 1, 'Active');

    DECLARE @NewId INT = SCOPE_IDENTITY();

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
    WHERE AlertEventId = @NewId;
END;
