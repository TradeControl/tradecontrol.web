CREATE VIEW [Cash].[vwCategoryPrimaryParent]
AS
WITH opts AS
(
    SELECT TOP (1)
        NetProfitCode,
        VatCategoryCode
    FROM App.tbOptions
),
roots AS
(
    SELECT NetProfitCode AS RootCode, CAST(N'Profit' AS nvarchar(10)) AS PrimaryKind
    FROM opts WHERE NetProfitCode IS NOT NULL
    UNION ALL
    SELECT VatCategoryCode, N'VAT'
    FROM opts WHERE VatCategoryCode IS NOT NULL
),
paths AS
(
    SELECT r.RootCode, r.PrimaryKind, t.ParentCode, t.ChildCode, 1 AS Depth
    FROM roots r
    JOIN Cash.tbCategoryTotal t ON t.ParentCode = r.RootCode

    UNION ALL

    SELECT p.RootCode, p.PrimaryKind, t.ParentCode, t.ChildCode, p.Depth + 1
    FROM paths p
    JOIN Cash.tbCategoryTotal t ON t.ParentCode = p.ChildCode
),
ranked AS
(
    SELECT
        p.ChildCode,
        p.ParentCode,
        p.RootCode,
        p.PrimaryKind,
        p.Depth,
        ROW_NUMBER() OVER
        (
            PARTITION BY p.ChildCode
            ORDER BY p.Depth, pc.DisplayOrder, pc.Category, pc.CategoryCode
        ) AS rn
    FROM paths p
    JOIN Cash.tbCategory pc ON pc.CategoryCode = p.ParentCode
),
parentAgg AS
(
    SELECT ChildCode, COUNT(*) AS ParentCount
    FROM Cash.tbCategoryTotal
    GROUP BY ChildCode
)
SELECT
    r.ChildCode,
    r.ParentCode,
    r.RootCode,
    r.PrimaryKind,
    r.Depth,
    r.rn,
    pa.ParentCount
FROM ranked r
LEFT JOIN parentAgg pa
  ON pa.ChildCode = r.ChildCode;