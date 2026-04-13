--0) Context: template / options snapshot
SELECT
    o.SubjectCode,
    o.NetProfitCode,
    o.VatCategoryCode,
    o.CoinTypeCode,
    o.UnitOfCharge
FROM App.tbOptions o;

----------------------------------------------------------------
-- Lookup sheets (so numeric codes are understandable in Excel)
----------------------------------------------------------------

--L1) CategoryType enum
SELECT CategoryTypeCode, CategoryType
FROM Cash.tbCategoryType
ORDER BY CategoryTypeCode;

--L2) CashPolarity enum
SELECT CashPolarityCode, CashPolarity
FROM Cash.tbPolarity
ORDER BY CashPolarityCode;

--L3) CashType enum
SELECT CashTypeCode, CashType
FROM Cash.tbType
ORDER BY CashTypeCode;

--L4) TaxTagClass enum
SELECT TagClassCode, TagClass
FROM Cash.tbTaxTagClass
ORDER BY TagClassCode;

--1) Category tree (totals rollup structure)
SELECT
    ct.ParentCode,
    p.Category AS ParentCategory,
    p.CategoryTypeCode AS ParentTypeCode,
    pt.CategoryType AS ParentType,
    ct.ChildCode,
    c.Category AS ChildCategory,
    c.CategoryTypeCode AS ChildTypeCode,
    ct2.CategoryType AS ChildType
FROM Cash.tbCategoryTotal AS ct
JOIN Cash.tbCategory AS p
    ON p.CategoryCode = ct.ParentCode
JOIN Cash.tbCategory AS c
    ON c.CategoryCode = ct.ChildCode
LEFT JOIN Cash.tbCategoryType pt
    ON pt.CategoryTypeCode = p.CategoryTypeCode
LEFT JOIN Cash.tbCategoryType ct2
    ON ct2.CategoryTypeCode = c.CategoryTypeCode
WHERE p.IsEnabled = 1
  AND c.IsEnabled = 1
ORDER BY ct.ParentCode, ct.ChildCode;

--2) All categories (decoded)
SELECT
    c.CategoryCode,
    c.Category,
    c.CategoryTypeCode,
    ct.CategoryType,
    c.CashPolarityCode,
    p.CashPolarity,
    c.CashTypeCode,
    t.CashType,
    c.DisplayOrder,
    c.IsEnabled
FROM Cash.tbCategory AS c
LEFT JOIN Cash.tbCategoryType ct
    ON ct.CategoryTypeCode = c.CategoryTypeCode
LEFT JOIN Cash.tbPolarity p
    ON p.CashPolarityCode = c.CashPolarityCode
LEFT JOIN Cash.tbType t
    ON t.CashTypeCode = c.CashTypeCode
WHERE c.IsEnabled = 1
ORDER BY c.CategoryTypeCode, c.CategoryCode;

--3) Cash codes (decoded via category)
SELECT
    cc.CashCode,
    cc.CashDescription,
    cc.CategoryCode,
    c.Category,
    c.CashPolarityCode,
    p.CashPolarity,
    c.CashTypeCode,
    t.CashType,
    cc.TaxCode,
    cc.IsEnabled
FROM Cash.tbCode AS cc
JOIN Cash.tbCategory AS c
    ON c.CategoryCode = cc.CategoryCode
LEFT JOIN Cash.tbPolarity p
    ON p.CashPolarityCode = c.CashPolarityCode
LEFT JOIN Cash.tbType t
    ON t.CashTypeCode = c.CashTypeCode
WHERE cc.IsEnabled = 1
ORDER BY cc.CategoryCode, cc.CashCode;

--4) Disconnected nominal categories (not part of any total)
SELECT DISTINCT
    cat.CategoryCode,
    cat.Category
FROM Cash.tbCategory AS cat
LEFT JOIN Cash.tbCategoryTotal AS ct
    ON ct.ChildCode = cat.CategoryCode
WHERE cat.CategoryTypeCode = 0
  AND cat.IsEnabled = 1
  AND ct.ParentCode IS NULL
ORDER BY cat.CategoryCode;

----------------------------------------------------------------
--5) ITSA sources + tags + (optional) mappings
--   Filter to ITSA so the AI stays on-topic.
----------------------------------------------------------------

/*
NOTE:
This script can optionally seed provisional ITSA mappings into Cash.tbTaxTagMap so that query 5c
produces real output for review. Keep this as a script-level aid; lift into templates only once confirmed.
*/

DECLARE @SeedMappings bit = 1;

IF @SeedMappings = 1
BEGIN
	-- Remove previous provisional mappings so the script is repeatable.
	DELETE FROM Cash.tbTaxTagMap
	WHERE TaxSourceCode IN ('UK-ITSA-SE-QU', 'UK-ITSA-SE-EOPS');

	----------------------------------------------------------------
	-- Provisional mappings (Slice 2)
	-- Keep these intentionally small and structural:
	--  - Category mappings where categories exist.
	--  - CashCode mappings only for finance split.
	----------------------------------------------------------------
	INSERT INTO Cash.tbTaxTagMap
		(TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled)
	VALUES
		-- QU: income + core costs (from proc_Template_BASE_MIN_2026)
		('UK-ITSA-SE-QU', 'turnover',        0, 'CT-TURNOV', '', 1),
		('UK-ITSA-SE-QU', 'otherIncome',     0, 'CT-OTHRIN', '', 1),
		('UK-ITSA-SE-QU', 'costOfGoods',     0, 'CT-CSTSAL', '', 1),
		('UK-ITSA-SE-QU', 'wagesSalaries',   0, 'CT-STAFFC', '', 1),

		-- QU: overhead headings (from ST STD extensions)
		('UK-ITSA-SE-QU', 'carVanExpenses',       0, 'CA-MOTOR',  '', 1),
		('UK-ITSA-SE-QU', 'travelExpenses',       0, 'CA-TRAVEL', '', 1),
		('UK-ITSA-SE-QU', 'premisesRunningCosts', 0, 'CA-PREMS',  '', 1),
		('UK-ITSA-SE-QU', 'adminCosts',           0, 'CA-ADMIN',  '', 1),

		-- QU: finance split by cash code (more accurate than mapping CA-FINANCE)
		('UK-ITSA-SE-QU', 'interestOnLoans',      1, '', 'CC-LOINT', 1),
		('UK-ITSA-SE-QU', 'financialCharges',     1, '', 'CC-FINCH', 1),

		-- QU: professional fees + advertising (your STD currently puts these under CA-ADMIN)
		('UK-ITSA-SE-QU', 'professionalFees',     1, '', 'CC-PROF', 1),
		('UK-ITSA-SE-QU', 'advertisingMarketing', 1, '', 'CC-ADVT', 1),

		-- QU: catch-all (keep narrow to avoid overlaps; expand later)
		('UK-ITSA-SE-QU', 'otherExpenses',        1, '', 'CC-ADMIN', 1),

		-- EOPS: same set for now (Slice 2 is “accounts totals”; derived/disallowables later)
		('UK-ITSA-SE-EOPS', 'turnover',        0, 'CT-TURNOV', '', 1),
		('UK-ITSA-SE-EOPS', 'otherIncome',     0, 'CT-OTHRIN', '', 1),
		('UK-ITSA-SE-EOPS', 'costOfGoods',     0, 'CT-CSTSAL', '', 1),
		('UK-ITSA-SE-EOPS', 'wagesSalaries',   0, 'CT-STAFFC', '', 1),

		('UK-ITSA-SE-EOPS', 'carVanExpenses',       0, 'CA-MOTOR',  '', 1),
		('UK-ITSA-SE-EOPS', 'travelExpenses',       0, 'CA-TRAVEL', '', 1),
		('UK-ITSA-SE-EOPS', 'premisesRunningCosts', 0, 'CA-PREMS',  '', 1),
		('UK-ITSA-SE-EOPS', 'adminCosts',           0, 'CA-ADMIN',  '', 1),

		('UK-ITSA-SE-EOPS', 'interestOnLoans',      1, '', 'CC-LOINT', 1),
		('UK-ITSA-SE-EOPS', 'financialCharges',     1, '', 'CC-FINCH', 1),
		('UK-ITSA-SE-EOPS', 'professionalFees',     1, '', 'CC-PROF', 1),
		('UK-ITSA-SE-EOPS', 'advertisingMarketing', 1, '', 'CC-ADVT', 1),
		('UK-ITSA-SE-EOPS', 'otherExpenses',        1, '', 'CC-ADMIN', 1);

	-- Validate (warnings go to event log; errors throw)
	EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = 'UK-ITSA-SE-QU';
	EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = 'UK-ITSA-SE-EOPS';
END;

--5a) ITSA tax tag sources
SELECT
    s.TaxSourceCode,
    s.JurisdictionCode,
    s.SourceName,
    s.SourceDescription,
    s.IsEnabled
FROM Cash.tbTaxTagSource s
WHERE s.IsEnabled = 1
  AND s.TaxSourceCode LIKE 'UK-ITSA-%'
ORDER BY s.TaxSourceCode;

--5b) ITSA tax tags (decoded TagClass)
SELECT
    tt.TaxSourceCode,
    tt.TagCode,
    tt.TagName,
    tt.TagClassCode,
    tc.TagClass,
    tt.DisplayOrder,
    tt.IsEnabled
FROM Cash.tbTaxTag AS tt
LEFT JOIN Cash.tbTaxTagClass tc
    ON tc.TagClassCode = tt.TagClassCode
WHERE tt.IsEnabled = 1
  AND tt.TaxSourceCode LIKE 'UK-ITSA-%'
ORDER BY tt.TaxSourceCode, tt.DisplayOrder, tt.TagCode;

--5c) ITSA tax tag mappings (likely empty at Slice 2 start, that is OK)
SELECT
    tm.TaxSourceCode,
    tm.TagCode,
    tt.TagName,
    tm.MapTypeCode,
    tm.CategoryCode,
    tm.CashCode,
    tm.IsEnabled
FROM Cash.tbTaxTagMap AS tm
JOIN Cash.tbTaxTag AS tt
    ON tt.TaxSourceCode = tm.TaxSourceCode
   AND tt.TagCode = tm.TagCode
WHERE tm.IsEnabled = 1
  AND tm.TaxSourceCode LIKE 'UK-ITSA-%'
ORDER BY tm.TaxSourceCode, tm.TagCode, tm.MapTypeCode, tm.CategoryCode, tm.CashCode;
