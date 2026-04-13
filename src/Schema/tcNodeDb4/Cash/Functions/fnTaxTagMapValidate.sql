CREATE FUNCTION Cash.fnTaxTagMapValidate
(
    @TaxSourceCode NVARCHAR(10)
)
RETURNS @Result TABLE
(
    IsError        BIT           NOT NULL,
    TagCode        NVARCHAR(20)  NULL,
    TagName        NVARCHAR(100) NULL,
    CashCode       NVARCHAR(50)  NULL,
    CategoryCode   NVARCHAR(10)  NULL,
    HitCount       INT           NULL,
    Message        NVARCHAR(4000) NOT NULL
)
AS
BEGIN
    DECLARE @MapRows TABLE
    (
        TaxSourceCode NVARCHAR(10) NOT NULL,
        TagCode       NVARCHAR(20) NOT NULL,
        MapTypeCode   TINYINT      NOT NULL,
        CategoryCode  NVARCHAR(10) NULL,
        CashCode      NVARCHAR(50) NULL
    );

    DECLARE @EffectiveCash TABLE
    (
        TaxSourceCode     NVARCHAR(10) NOT NULL,
        TagCode           NVARCHAR(20) NOT NULL,
        RootCategoryCode  NVARCHAR(10) NULL,
        CashCode          NVARCHAR(50) NOT NULL
    );

    DECLARE @MappedCash TABLE
    (
        TaxSourceCode NVARCHAR(10) NOT NULL,
        CashCode      NVARCHAR(50) NOT NULL
    );

    INSERT INTO @MapRows (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode)
    SELECT
        tm.TaxSourceCode,
        tm.TagCode,
        tm.MapTypeCode,
        NULLIF(tm.CategoryCode, ''),
        NULLIF(tm.CashCode, '')
    FROM Cash.tbTaxTagMap tm
    WHERE tm.TaxSourceCode = @TaxSourceCode
      AND tm.IsEnabled = 1;

    ----------------------------------------------------------------
    -- Warnings: orphaned map rows (category/cash code deleted/renamed)
    ----------------------------------------------------------------
    INSERT INTO @Result (IsError, TagCode, TagName, CashCode, CategoryCode, HitCount, Message)
    SELECT
        CONVERT(BIT, 0) AS IsError,
        mr.TagCode,
        tt.TagName,
        CAST(NULL AS NVARCHAR(50)) AS CashCode,
        mr.CategoryCode,
        CAST(NULL AS INT) AS HitCount,
        N'Map row references a CategoryCode that does not exist in Cash.tbCategory.' AS Message
    FROM @MapRows mr
    LEFT JOIN Cash.tbTaxTag tt
        ON tt.TaxSourceCode = mr.TaxSourceCode
       AND tt.TagCode = mr.TagCode
    WHERE mr.MapTypeCode = 0
      AND mr.CategoryCode IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM Cash.tbCategory c WHERE c.CategoryCode = mr.CategoryCode);

    INSERT INTO @Result (IsError, TagCode, TagName, CashCode, CategoryCode, HitCount, Message)
    SELECT
        CONVERT(BIT, 0) AS IsError,
        mr.TagCode,
        tt.TagName,
        mr.CashCode,
        CAST(NULL AS NVARCHAR(10)) AS CategoryCode,
        CAST(NULL AS INT) AS HitCount,
        N'Map row references a CashCode that does not exist in Cash.tbCode.' AS Message
    FROM @MapRows mr
    LEFT JOIN Cash.tbTaxTag tt
        ON tt.TaxSourceCode = mr.TaxSourceCode
       AND tt.TagCode = mr.TagCode
    WHERE mr.MapTypeCode = 1
      AND mr.CashCode IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM Cash.tbCode cc WHERE cc.CashCode = mr.CashCode);

    ;WITH CatSeed AS
    (
        SELECT
            mr.TaxSourceCode,
            mr.TagCode,
            mr.CategoryCode AS RootCategoryCode,
            mr.CategoryCode AS CategoryCode
        FROM @MapRows mr
        WHERE mr.MapTypeCode = 0
          AND mr.CategoryCode IS NOT NULL
    ),
    CatTree AS
    (
        SELECT
            cs.TaxSourceCode,
            cs.TagCode,
            cs.RootCategoryCode,
            cs.CategoryCode
        FROM CatSeed cs

        UNION ALL

        SELECT
            ct.TaxSourceCode,
            ct.TagCode,
            ct.RootCategoryCode,
            rel.ChildCode AS CategoryCode
        FROM CatTree ct
        JOIN Cash.tbCategoryTotal rel
            ON rel.ParentCode = ct.CategoryCode
    ),
    CashFromCats AS
    (
        SELECT DISTINCT
            ct.TaxSourceCode,
            ct.TagCode,
            ct.RootCategoryCode,
            cc.CashCode
        FROM CatTree ct
        JOIN Cash.tbCode cc
            ON cc.CategoryCode = ct.CategoryCode
        WHERE cc.IsEnabled = 1
    ),
    CashFromCodes AS
    (
        SELECT
            mr.TaxSourceCode,
            mr.TagCode,
            CAST(NULL AS NVARCHAR(10)) AS RootCategoryCode,
            mr.CashCode AS CashCode
        FROM @MapRows mr
        WHERE mr.MapTypeCode = 1
          AND mr.CashCode IS NOT NULL
    ),
    EffectiveCash AS
    (
        SELECT TaxSourceCode, TagCode, RootCategoryCode, CashCode
        FROM CashFromCats

        UNION ALL

        SELECT TaxSourceCode, TagCode, RootCategoryCode, CashCode
        FROM CashFromCodes
    )
    INSERT INTO @EffectiveCash (TaxSourceCode, TagCode, RootCategoryCode, CashCode)
    SELECT
        ec.TaxSourceCode,
        ec.TagCode,
        ec.RootCategoryCode,
        ec.CashCode
    FROM EffectiveCash ec;

    INSERT INTO @Result (IsError, TagCode, TagName, CashCode, CategoryCode, HitCount, Message)
    SELECT
        CONVERT(BIT, 1) AS IsError,
        d.TagCode,
        t.TagName,
        d.CashCode,
        CAST(NULL AS NVARCHAR(10)) AS CategoryCode,
        d.HitCount,
        N'CashCode is included multiple times for the same tag (overlapping category/cash mappings).' AS Message
    FROM
    (
        SELECT
            ec.TaxSourceCode,
            ec.TagCode,
            ec.CashCode,
            COUNT(*) AS HitCount
        FROM @EffectiveCash ec
        GROUP BY ec.TaxSourceCode, ec.TagCode, ec.CashCode
        HAVING COUNT(*) > 1
    ) d
    LEFT JOIN Cash.tbTaxTag t
        ON t.TaxSourceCode = d.TaxSourceCode
       AND t.TagCode = d.TagCode;

    INSERT INTO @MappedCash (TaxSourceCode, CashCode)
    SELECT DISTINCT
        ec.TaxSourceCode,
        ec.CashCode
    FROM @EffectiveCash ec;

    ;WITH DisconnectedCategory AS
    (
        SELECT DISTINCT
            cat.CategoryCode
        FROM Cash.tbCategory cat
        LEFT JOIN Cash.tbCategoryTotal ct
            ON ct.ChildCode = cat.CategoryCode
        WHERE ct.ParentCode IS NULL
    )
    INSERT INTO @Result (IsError, TagCode, TagName, CashCode, CategoryCode, HitCount, Message)
    SELECT
        CONVERT(BIT, 0) AS IsError,
        CAST(NULL AS NVARCHAR(20)) AS TagCode,
        CAST(NULL AS NVARCHAR(100)) AS TagName,
        cc.CashCode,
        cc.CategoryCode,
        CAST(NULL AS INT) AS HitCount,
        N'CashCode is not mapped to any MTD tag for this source.' AS Message
    FROM Cash.tbCode cc
    LEFT JOIN @MappedCash mc
        ON mc.TaxSourceCode = @TaxSourceCode
       AND mc.CashCode = cc.CashCode
    WHERE cc.IsEnabled = 1
      AND mc.CashCode IS NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM DisconnectedCategory d
          WHERE d.CategoryCode = cc.CategoryCode
      );

    RETURN;
END;
GO
