CREATE FUNCTION [Cash].[fnCategoryNamespaceInContext]
(
    @CategoryCode varchar(10),
    @ParentCode   varchar(10) = NULL
)
RETURNS nvarchar(MAX)
AS
BEGIN
    IF @CategoryCode IS NULL OR LEN(@CategoryCode) = 0 RETURN NULL;
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = @CategoryCode) RETURN NULL;

    DECLARE @result nvarchar(MAX);

    -- If the node is not linked at all, treat as Disconnected
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategoryTotal WHERE ParentCode = @CategoryCode OR ChildCode = @CategoryCode)
    BEGIN
        SELECT @result = N'Disconnected.' + REPLACE(c.Category, ' ', '_')
        FROM Cash.tbCategory c
        WHERE c.CategoryCode = @CategoryCode;

        RETURN [App].[fnToAlphaNumeric](@result);
    END

    ;WITH ParentGiven AS
    (
        -- Optional explicit first step parent mapping (only for the selected child)
        SELECT t.ChildCode, t.ParentCode, rn = 1
        FROM Cash.tbCategoryTotal t
        WHERE @ParentCode IS NOT NULL
          AND t.ChildCode = @CategoryCode
          AND t.ParentCode = @ParentCode
    ),
    ParentRank AS
    (
        -- Deterministic parent rank for all children
        SELECT
            t.ChildCode,
            t.ParentCode,
            rn = ROW_NUMBER() OVER (
                    PARTITION BY t.ChildCode
                    ORDER BY p.DisplayOrder, p.Category, p.CategoryCode
                 )
        FROM Cash.tbCategoryTotal t
        JOIN Cash.tbCategory p ON p.CategoryCode = t.ParentCode
    ),
    ParentMap AS
    (
        -- Use the explicit parent for the first step if present; otherwise the deterministic rn=1
        SELECT ChildCode, ParentCode
        FROM ParentGiven

        UNION ALL

        SELECT pr.ChildCode, pr.ParentCode
        FROM ParentRank pr
        WHERE pr.rn = 1
          AND NOT EXISTS (SELECT 1 FROM ParentGiven g WHERE g.ChildCode = pr.ChildCode)
    ),
    ParentCTE AS
    (
        -- Start at the node itself
        SELECT
            c.CategoryCode,
            REPLACE(c.Category, ' ', '_') AS [Path],
            CAST(0 AS int) AS Depth
        FROM Cash.tbCategory c
        WHERE c.CategoryCode = @CategoryCode

        UNION ALL

        -- Walk up using ParentMap (first step honors @ParentCode if provided)
        SELECT
            pm.ParentCode,
            REPLACE(p.Category, ' ', '_') + N'.' + cte.[Path],
            cte.Depth + 1
        FROM ParentCTE cte
        JOIN ParentMap pm
          ON pm.ChildCode = cte.CategoryCode
        JOIN Cash.tbCategory p
          ON p.CategoryCode = pm.ParentCode
    )
    SELECT TOP (1) @result = cte.[Path]
    FROM ParentCTE cte
    ORDER BY cte.Depth DESC;

    RETURN [App].[fnToAlphaNumeric](@result);
END