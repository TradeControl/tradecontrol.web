SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @SeedMappings bit = 1;

------------------------------------------------------------------------------
-- Optional: keep seeding behavior identical to MTDSoleTraderMappingEnquiry.sql
------------------------------------------------------------------------------
IF @SeedMappings = 1
BEGIN
	DELETE FROM Cash.tbTaxTagMap
	WHERE TaxSourceCode IN ('UK-ITSA-SE-QU', 'UK-ITSA-SE-EOPS');

	INSERT INTO Cash.tbTaxTagMap
		(TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled)
	VALUES
		('UK-ITSA-SE-QU', 'turnover',        0, 'CT-TURNOV', '', 1),
		('UK-ITSA-SE-QU', 'otherIncome',     0, 'CT-OTHRIN', '', 1),
		('UK-ITSA-SE-QU', 'costOfGoods',     0, 'CT-CSTSAL', '', 1),
		('UK-ITSA-SE-QU', 'wagesSalaries',   0, 'CT-STAFFC', '', 1),

		('UK-ITSA-SE-QU', 'carVanExpenses',       0, 'CA-MOTOR',  '', 1),
		('UK-ITSA-SE-QU', 'travelExpenses',       0, 'CA-TRAVEL', '', 1),
		('UK-ITSA-SE-QU', 'premisesRunningCosts', 0, 'CA-PREMS',  '', 1),
		('UK-ITSA-SE-QU', 'adminCosts',           0, 'CA-ADMIN',  '', 1),

		('UK-ITSA-SE-QU', 'interestOnLoans',      1, '', 'CC-LOINT', 1),
		('UK-ITSA-SE-QU', 'financialCharges',     1, '', 'CC-FINCH', 1),

		('UK-ITSA-SE-QU', 'professionalFees',     1, '', 'CC-PROF', 1),
		('UK-ITSA-SE-QU', 'advertisingMarketing', 1, '', 'CC-ADVT', 1),

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
		('UK-ITSA-SE-EOPS', 'advertisingMarketing', 1, '', 'CC-ADVT', 1);

	EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = 'UK-ITSA-SE-QU';
	EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = 'UK-ITSA-SE-EOPS';
END;

------------------------------------------------------------------------------
-- Single XML document export for AI review
------------------------------------------------------------------------------
;WITH overhead_categories AS
(
	SELECT ct.ChildCode AS CategoryCode
	FROM Cash.tbCategoryTotal ct
	WHERE ct.ParentCode = 'CT-OVERHD'
),
overhead_cashcodes AS
(
	SELECT cc.CashCode, cc.CashDescription, cc.CategoryCode
	FROM Cash.tbCode cc
	JOIN overhead_categories oc
		ON oc.CategoryCode = cc.CategoryCode
	WHERE cc.IsEnabled = 1
),
mapped_cashcodes AS
(
	SELECT TaxSourceCode, CashCode
	FROM Cash.tbTaxTagMap
	WHERE IsEnabled = 1
	  AND MapTypeCode = 1
	  AND TaxSourceCode IN ('UK-ITSA-SE-QU', 'UK-ITSA-SE-EOPS')
	  AND CashCode <> ''
),
mapped_categories AS
(
	SELECT TaxSourceCode, CategoryCode
	FROM Cash.tbTaxTagMap
	WHERE IsEnabled = 1
	  AND MapTypeCode = 0
	  AND TaxSourceCode IN ('UK-ITSA-SE-QU', 'UK-ITSA-SE-EOPS')
	  AND CategoryCode <> ''
),
residual_overheads AS
(
	SELECT
		ts.TaxSourceCode,
		oc.CashCode,
		oc.CashDescription,
		oc.CategoryCode,
		Category = cat.Category
	FROM (VALUES ('UK-ITSA-SE-QU'), ('UK-ITSA-SE-EOPS')) ts(TaxSourceCode)
	JOIN overhead_cashcodes oc
		ON 1 = 1
	JOIN Cash.tbCategory cat
		ON cat.CategoryCode = oc.CategoryCode
	LEFT JOIN mapped_cashcodes mc
		ON mc.TaxSourceCode = ts.TaxSourceCode
	   AND mc.CashCode = oc.CashCode
	LEFT JOIN mapped_categories mcat
		ON mcat.TaxSourceCode = ts.TaxSourceCode
	   AND mcat.CategoryCode = oc.CategoryCode
	WHERE mc.CashCode IS NULL
	  AND mcat.CategoryCode IS NULL
)
SELECT
	Meta =
	(
		SELECT
			ExportName = 'MTD ITSA Sole Trader Mapping Enquiry (Slice 2)',
			ExportUtc = SYSUTCDATETIME(),
			SeedMappings = @SeedMappings
		FOR XML PATH('Meta'), TYPE
	),

	Options =
	(
		SELECT
			o.SubjectCode,
			o.NetProfitCode,
			o.VatCategoryCode,
			o.CoinTypeCode,
			o.UnitOfCharge
		FROM App.tbOptions o
		FOR XML PATH('Options'), TYPE
	),

	Lookups =
	(
		SELECT
			(SELECT CategoryTypeCode, CategoryType FROM Cash.tbCategoryType ORDER BY CategoryTypeCode FOR XML PATH('CategoryType'), ROOT('CategoryTypes'), TYPE),
			(SELECT CashPolarityCode, CashPolarity FROM Cash.tbPolarity ORDER BY CashPolarityCode FOR XML PATH('CashPolarity'), ROOT('CashPolarities'), TYPE),
			(SELECT CashTypeCode, CashType FROM Cash.tbType ORDER BY CashTypeCode FOR XML PATH('CashType'), ROOT('CashTypes'), TYPE),
			(SELECT TagClassCode, TagClass FROM Cash.tbTaxTagClass ORDER BY TagClassCode FOR XML PATH('TaxTagClass'), ROOT('TaxTagClasses'), TYPE)
		FOR XML PATH('Lookups'), TYPE
	),

	CategoryTotals =
	(
		SELECT
			ct.ParentCode,
			ParentCategory = p.Category,
			ParentTypeCode = p.CategoryTypeCode,
			ParentType = pt.CategoryType,
			ct.ChildCode,
			ChildCategory = c.Category,
			ChildTypeCode = c.CategoryTypeCode,
			ChildType = ct2.CategoryType
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
		ORDER BY ct.ParentCode, ct.ChildCode
		FOR XML PATH('CategoryTotal'), ROOT('CategoryTotals'), TYPE
	),

	Categories =
	(
		SELECT
			c.CategoryCode,
			c.Category,
			c.CategoryTypeCode,
			CategoryType = ct.CategoryType,
			c.CashPolarityCode,
			CashPolarity = p.CashPolarity,
			c.CashTypeCode,
			CashType = t.CashType,
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
		ORDER BY c.CategoryTypeCode, c.CategoryCode
		FOR XML PATH('Category'), ROOT('Categories'), TYPE
	),

	CashCodes =
	(
		SELECT
			cc.CashCode,
			cc.CashDescription,
			cc.CategoryCode,
			Category = c.Category,
			c.CashPolarityCode,
			CashPolarity = p.CashPolarity,
			c.CashTypeCode,
			CashType = t.CashType,
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
		ORDER BY cc.CategoryCode, cc.CashCode
		FOR XML PATH('CashCode'), ROOT('CashCodes'), TYPE
	),

	DisconnectedNominals =
	(
		SELECT DISTINCT
			cat.CategoryCode,
			cat.Category
		FROM Cash.tbCategory AS cat
		LEFT JOIN Cash.tbCategoryTotal AS ct
			ON ct.ChildCode = cat.CategoryCode
		WHERE cat.CategoryTypeCode = 0
		  AND cat.IsEnabled = 1
		  AND ct.ParentCode IS NULL
		ORDER BY cat.CategoryCode
		FOR XML PATH('DisconnectedNominal'), ROOT('DisconnectedNominals'), TYPE
	),

	Itsa =
	(
		SELECT
			Sources =
			(
				SELECT
					s.TaxSourceCode,
					s.JurisdictionCode,
					s.SourceName,
					s.SourceDescription,
					s.IsEnabled
				FROM Cash.tbTaxTagSource s
				WHERE s.IsEnabled = 1
				  AND s.TaxSourceCode LIKE 'UK-ITSA-%'
				ORDER BY s.TaxSourceCode
				FOR XML PATH('Source'), ROOT('Sources'), TYPE
			),
			Tags =
			(
				SELECT
					tt.TaxSourceCode,
					tt.TagCode,
					tt.TagName,
					tt.TagClassCode,
					TagClass = tc.TagClass,
					tt.DisplayOrder,
					tt.IsEnabled
				FROM Cash.tbTaxTag AS tt
				LEFT JOIN Cash.tbTaxTagClass tc
					ON tc.TagClassCode = tt.TagClassCode
				WHERE tt.IsEnabled = 1
				  AND tt.TaxSourceCode LIKE 'UK-ITSA-%'
				ORDER BY tt.TaxSourceCode, tt.DisplayOrder, tt.TagCode
				FOR XML PATH('Tag'), ROOT('Tags'), TYPE
			),
			Maps =
			(
				SELECT
					tm.TaxSourceCode,
					tm.TagCode,
					TagName = tt.TagName,
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
				ORDER BY tm.TaxSourceCode, tm.TagCode, tm.MapTypeCode, tm.CategoryCode, tm.CashCode
				FOR XML PATH('Map'), ROOT('Maps'), TYPE
			)
		FOR XML PATH('ITSA'), TYPE
	),

	ResidualOverheads =
	(
		SELECT
			r.TaxSourceCode,
			r.CashCode,
			r.CashDescription,
			r.CategoryCode,
			r.Category
		FROM residual_overheads r
		ORDER BY r.TaxSourceCode, r.CategoryCode, r.CashCode
		FOR XML PATH('ResidualOverhead'), ROOT('ResidualOverheads'), TYPE
	)

FOR XML PATH('MTD'), ROOT('MTD_ITSA_SoleTrader_Enquiry'), TYPE;
