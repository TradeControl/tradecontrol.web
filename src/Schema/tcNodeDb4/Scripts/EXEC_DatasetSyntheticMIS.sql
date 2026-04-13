SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE
    @IsCompany bit = 0; -- set to 0 for Sole Trader scenarios

DECLARE 
    @EnableWages bit = CASE WHEN @IsCompany = 1 THEN 1 ELSE 0 END
    , @EnableAssets bit = CASE WHEN @IsCompany = 1 THEN 1 ELSE 0 END;

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
	 (N'VAT=0, PriceRatio=0.5 (loss)',   0, 0.5)
	, (N'VAT=1, PriceRatio=0.5 (loss)',   1, 0.5)
	, (N'VAT=0, PriceRatio=3.0 (profit)', 0, 3.0)
	, (N'VAT=1, PriceRatio=3.0 (profit)', 1, 3.0)
    ;

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
	BusinessTax decimal(18,2) NULL,
	ProfitAfterTax decimal(18,2) NULL,
	CapitalMovement decimal(18,2) NULL,
	OpeningPosition decimal(18,2) NULL,
	OpeningAccountPosition decimal(18,2) NULL,
	CapitalDelta decimal(18,2) NULL,
	Variance decimal(18,2) NULL
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
		@IsCompany = @IsCompany,
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
		@EnableWages = @EnableWages,
		@EnableExpenses = 1,
		@EnableAssets = @EnableAssets,
		@EnableTax = 1,
		@EnableTransfers = 1,
		@EnableOpeningBalance = 1;

	INSERT INTO #EquityRecon
	(
		ScenarioId, ScenarioName, YearNumber, [Description],
		OpeningCapital, ClosingCapital, Profit, BusinessTax, ProfitAfterTax,
		CapitalMovement, OpeningPosition, OpeningAccountPosition, CapitalDelta, Variance
	)
	SELECT
		@ScenarioId,
		@ScenarioName,
		YearNumber,
		[Description],
		OpeningCapital,
		ClosingCapital,
		Profit,
		BusinessTax,
		ProfitAfterTax,
		CAST(CapitalMovement AS decimal(18,2)),
		CAST(OpeningPosition AS decimal(18,2)),
		CAST(OpeningAccountPosition AS decimal(18,2)),
		CapitalDelta,
		Variance
	FROM Cash.vwEquityReconciliationByYear;

	FETCH NEXT FROM cur INTO @ScenarioId, @ScenarioName, @IsVatRegistered, @PriceRatio;
END

CLOSE cur;
DEALLOCATE cur;

SELECT *
FROM #EquityRecon
ORDER BY ScenarioId, YearNumber;
