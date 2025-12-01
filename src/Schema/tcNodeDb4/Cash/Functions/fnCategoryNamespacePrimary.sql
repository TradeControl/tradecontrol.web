CREATE FUNCTION [Cash].[fnCategoryNamespacePrimary]
(
    @CategoryCode varchar(10)
)
RETURNS nvarchar(MAX)
AS
BEGIN
    IF @CategoryCode IS NULL OR LEN(@CategoryCode) = 0 RETURN NULL;
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = @CategoryCode) RETURN NULL;

    DECLARE @result nvarchar(MAX);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategoryTotal WHERE ParentCode = @CategoryCode OR ChildCode = @CategoryCode)
    BEGIN
        SELECT @result = N'Disconnected.' + REPLACE(c.Category, ' ', '_')
        FROM Cash.tbCategory c
        WHERE c.CategoryCode = @CategoryCode;

        RETURN [App].[fnToAlphaNumeric](@result);
    END

    ;WITH ParentMap AS
    (
        SELECT ChildCode, ParentCode
        FROM Cash.vwCategoryPrimaryParent
        WHERE rn = 1
    ),
    ParentCTE AS
    (
        SELECT
            c.CategoryCode,
            REPLACE(c.Category, ' ', '_') AS [Path],
            CAST(0 AS int) AS Depth
        FROM Cash.tbCategory c
        WHERE c.CategoryCode = @CategoryCode

        UNION ALL

        SELECT
            pm.ParentCode,
            REPLACE(p.Category, ' ', '_') + N'.' + cte.[Path],
            cte.Depth + 1
        FROM ParentCTE cte
        JOIN ParentMap pm ON pm.ChildCode = cte.CategoryCode
        JOIN Cash.tbCategory p ON p.CategoryCode = pm.ParentCode
    )
    SELECT TOP (1) @result = cte.[Path]
    FROM ParentCTE cte
    ORDER BY cte.Depth DESC;

    RETURN [App].[fnToAlphaNumeric](@result);
END