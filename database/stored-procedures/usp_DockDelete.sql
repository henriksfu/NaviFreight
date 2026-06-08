CREATE OR ALTER PROCEDURE dbo.usp_DockDelete
    @TenantId NVARCHAR(50),
    @YardId   INT,
    @DockId   INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE dbo.Docks
    FROM dbo.Docks d
    INNER JOIN dbo.Yards y ON y.YardId = d.YardId
    WHERE d.DockId   = @DockId
      AND d.YardId   = @YardId
      AND y.TenantId = @TenantId;

    SELECT @@ROWCOUNT AS AffectedRows;
END;
