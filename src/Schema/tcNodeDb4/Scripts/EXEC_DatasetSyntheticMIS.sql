SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('tempdb..#Scenarios') IS NOT NULL DROP TABLE #Scenarios;
CREATE TABLE #Scenarios
(
	ScenarioId int IDENTITY(1,1) PRIMARY KEY,
	ScenarioName nvarchar(100) NOT NULL,
	IsVatRegistered bit NOT NULL,
	PriceRatio decimal(18,7) NOT NULL
);

INSERT INTO #Scenarios (ScenarioName, IsVatRegistered, PriceRatio)
VALUES
	(N'VAT=0, PriceRatio=3.0 (profit)', 0, 3.0),
	(N'VAT=1, PriceRatio=3.0 (profit)', 1, 3.0),
	(N'VAT=0, PriceRatio=0.5 (loss)',   0, 0.5),
	(N'VAT=1, PriceRatio=0.5 (loss)',   1, 0.5);

IF OBJECT_ID('tempdb..#EquityRecon') IS NOT NULL DROP TABLE #EquityRecon;
CREATE TABLE #EquityRecon
(
	ScenarioId int NOT NULL,
	ScenarioName nvarchar(100) NOT NULL,
	YearNumber smallint NOT NULL,
	[Description] nvarchar(50) NULL,
	OpeningCapital decimal(18,2) NULL,
	ClosingCapital decimal(18,2) NULL,
	Profit decimal(18,2) NULL,
	CorporationTax decimal(18,2) NULL,
	ProfitAfterTax decimal(18,2) NULL,
	CapitalInjection decimal(18,2) NULL,
	OpeningPosition decimal(18,2) NULL,
	CapitalDelta decimal(18,2) NULL,
	Difference decimal(18,2) NULL
);

DECLARE
	@ScenarioId int,
	@ScenarioName nvarchar(100),
	@IsVatRegistered bit,
	@PriceRatio decimal(18,7);

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
	SELECT ScenarioId, ScenarioName, IsVatRegistered, PriceRatio
	FROM #Scenarios
	ORDER BY ScenarioId;

OPEN cur;
FETCH NEXT FROM cur INTO @ScenarioId, @ScenarioName, @IsVatRegistered, @PriceRatio;

WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT CONCAT('Running scenario ', @ScenarioId, ': ', @ScenarioName);

	EXEC App.proc_DatasetSyntheticMIS
		@IsCompany = 1,
		@IsVatRegistered = @IsVatRegistered,
		@MisOrdersPerMonth = 2,
		@MonthsForward = 3,
		@PriceRatio = @PriceRatio,
		@QuantityRatio = 10,
		@FloatRatio = 0.25,
		@EnableProjects = 1,
		@EnableInvoices = 1,
		@EnableProjectPayments = 1,
		@EnablePayables = 1,
		@EnableMiscPayments = 1,
		@EnableWages = 1,
		@EnableExpenses = 1,
		@EnableAssets = 1,
		@EnableTax = 1,
		@EnableTransfers = 1,
		@EnableOpeningBalance = 1;

	INSERT INTO #EquityRecon
	(
		ScenarioId, ScenarioName, YearNumber, [Description],
		OpeningCapital, ClosingCapital, Profit, CorporationTax, ProfitAfterTax,
		CapitalInjection, OpeningPosition, CapitalDelta, Difference
	)
	SELECT
		@ScenarioId,
		@ScenarioName,
		YearNumber,
		[Description],
		OpeningCapital,
		ClosingCapital,
		Profit,
		CorporationTax,
		ProfitAfterTax,
		CAST(CapitalInjection AS decimal(18,2)),
		CAST(OpeningPosition AS decimal(18,2)),
		CapitalDelta,
		Difference
	FROM Cash.vwEquityReconciliationByYear;

	FETCH NEXT FROM cur INTO @ScenarioId, @ScenarioName, @IsVatRegistered, @PriceRatio;
END

CLOSE cur;
DEALLOCATE cur;

-- Summary: show first-year and total difference per scenario
SELECT
	ScenarioId,
	ScenarioName,
	FirstYear = MIN(YearNumber),
	FirstYearDifference = SUM(CASE WHEN YearNumber = (SELECT MIN(YearNumber) FROM #EquityRecon e2 WHERE e2.ScenarioId = e.ScenarioId) THEN Difference ELSE 0 END),
	TotalAbsDifference = SUM(ABS(Difference))
FROM #EquityRecon e
GROUP BY ScenarioId, ScenarioName
ORDER BY ScenarioId;

-- Full breakdown if needed
SELECT *
FROM #EquityRecon
ORDER BY ScenarioId, YearNumber;
