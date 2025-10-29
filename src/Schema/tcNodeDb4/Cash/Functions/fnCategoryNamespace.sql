CREATE FUNCTION [Cash].[fnCategoryNamespace]
(
    @CategoryCode varchar(10)
)
RETURNS nvarchar(MAX)
AS
BEGIN
    DECLARE @result nvarchar(MAX);

    IF @CategoryCode IS NULL OR LEN(@CategoryCode) = 0
        RETURN NULL;

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = @CategoryCode)
        RETURN NULL;

    IF NOT EXISTS (
        SELECT 1
        FROM Cash.tbCategoryTotal t
        WHERE t.ParentCode = @CategoryCode OR t.ChildCode = @CategoryCode
    )
    BEGIN
        SELECT @result = N'Disconnected.' + REPLACE(c.Category, ' ', '_')
        FROM Cash.tbCategory c
        WHERE c.CategoryCode = @CategoryCode;

        RETURN @result;
    END

    ;WITH ParentChoice AS
    (
        SELECT
            t.ChildCode,
            t.ParentCode,
            rn = ROW_NUMBER() OVER (
                PARTITION BY t.ChildCode
                ORDER BY p.DisplayOrder, p.Category, p.CategoryCode
            )
        FROM Cash.tbCategoryTotal t
        JOIN Cash.tbCategory p
            ON p.CategoryCode = t.ParentCode
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
            pc.ParentCode,
            REPLACE(p.Category, ' ', '_') + N'.' + cte.[Path],
            cte.Depth + 1
        FROM ParentCTE cte
        JOIN ParentChoice pc
            ON pc.ChildCode = cte.CategoryCode
           AND pc.rn = 1
        JOIN Cash.tbCategory p
            ON p.CategoryCode = pc.ParentCode
    )
    SELECT TOP (1) @result = cte.[Path]
    FROM ParentCTE cte
    ORDER BY cte.Depth DESC;

    RETURN [App].[fnToAlphaNumeric](@result);
END

