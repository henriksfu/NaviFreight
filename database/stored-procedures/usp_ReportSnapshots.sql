CREATE OR ALTER PROCEDURE dbo.usp_ReportSnapshots
    @TenantId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH LatestSnapshots AS
    (
        SELECT
            ReportName,
            MetricValue,
            ChangeLabel,
            ROW_NUMBER() OVER (PARTITION BY ReportName ORDER BY RecordedUtc DESC) AS RowNum
        FROM dbo.ReportSnapshots
        WHERE TenantId = @TenantId
    )
    SELECT
        ReportName,
        MetricValue AS Value,
        ChangeLabel
    FROM LatestSnapshots
    WHERE RowNum = 1
    ORDER BY ReportName;
END;
