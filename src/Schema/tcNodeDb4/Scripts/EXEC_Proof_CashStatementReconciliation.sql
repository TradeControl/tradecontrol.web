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

IF OBJECT_ID('tempdb..#ProofSummary') IS NOT NULL DROP TABLE #ProofSummary;
CREATE TABLE #ProofSummary
(
	ScenarioId int NOT NULL,
	ScenarioName nvarchar(100) NOT NULL,
	ProofName nvarchar(200) NOT NULL,
	Tolerance decimal(18,4) NOT NULL,
	YearCount int NOT NULL,
	CapitalDelta_Definition_MaxAbsError decimal(38,18) NULL,
	ProfitAfterTax_Definition_MaxAbsError decimal(38,18) NULL,
	Difference_Definition_MaxAbsError decimal(38,18) NULL,
	Difference_MaxAbs decimal(38,18) NULL,
	LossesCarriedForwardDelta_Definition_MaxAbsError decimal(38,18) NULL,
	LossesCarriedForward_NegativeCount int NULL,
	Status nvarchar(10) NOT NULL
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

	IF OBJECT_ID('tempdb..#Calc') IS NOT NULL DROP TABLE #Calc;
	SELECT
		CapitalDelta_Error = (r.CapitalDelta - (r.ClosingCapital - r.OpeningCapital)),
		ProfitAfterTax_Error = (r.ProfitAfterTax - (r.Profit - r.BusinessTax)),
		Residual_Error = (r.Difference - (r.CapitalDelta - (r.ProfitAfterTax + r.CapitalInjection + r.OpeningPosition))),
		Residual_Definition = (r.CapitalDelta - (r.ProfitAfterTax + r.CapitalInjection + r.OpeningPosition)),
		LossCF_Sign_Bad = CASE WHEN r.ClosingLossesCarriedForward < 0 THEN 1 ELSE 0 END,
		LossCF_Delta_Error = (r.LossesCarriedForwardDelta - (r.ClosingLossesCarriedForward - r.OpeningLossesCarriedForward))
	INTO #Calc
	FROM Cash.vwEquityReconciliationByYear r;

	DECLARE
		@Tol decimal(18,4) = 0.10,
		@YearCount int,
		@MaxAbs_CapitalDelta_Error decimal(38,18),
		@MaxAbs_ProfitAfterTax_Error decimal(38,18),
		@MaxAbs_Residual_Error decimal(38,18),
		@MaxAbs_Difference decimal(38,18),
		@MaxAbs_LossCF_Delta_Error decimal(38,18),
		@LossCF_Negative_Count int;

	SELECT
		@YearCount = COUNT(*),
		@MaxAbs_CapitalDelta_Error = MAX(ABS(CapitalDelta_Error)),
		@MaxAbs_ProfitAfterTax_Error = MAX(ABS(ProfitAfterTax_Error)),
		@MaxAbs_Residual_Error = MAX(ABS(Residual_Error)),
		@MaxAbs_Difference = MAX(ABS(Residual_Definition)),
		@MaxAbs_LossCF_Delta_Error = MAX(ABS(LossCF_Delta_Error)),
		@LossCF_Negative_Count = SUM(LossCF_Sign_Bad)
	FROM #Calc;

	INSERT INTO #ProofSummary
	(
		ScenarioId,
		ScenarioName,
		ProofName,
		Tolerance,
		YearCount,
		CapitalDelta_Definition_MaxAbsError,
		ProfitAfterTax_Definition_MaxAbsError,
		Difference_Definition_MaxAbsError,
		Difference_MaxAbs,
		LossesCarriedForwardDelta_Definition_MaxAbsError,
		LossesCarriedForward_NegativeCount,
		Status
	)
	VALUES
	(
		@ScenarioId,
		@ScenarioName,
		N'Cash Statement / Equity Reconciliation Proofs',
		@Tol,
		COALESCE(@YearCount, 0),
		@MaxAbs_CapitalDelta_Error,
		@MaxAbs_ProfitAfterTax_Error,
		@MaxAbs_Residual_Error,
		@MaxAbs_Difference,
		@MaxAbs_LossCF_Delta_Error,
		COALESCE(@LossCF_Negative_Count, 0),
		CASE
			WHEN COALESCE(@MaxAbs_CapitalDelta_Error, 0) > @Tol THEN N'FAIL'
			WHEN COALESCE(@MaxAbs_ProfitAfterTax_Error, 0) > @Tol THEN N'FAIL'
			WHEN COALESCE(@MaxAbs_Residual_Error, 0) > @Tol THEN N'FAIL'
			WHEN COALESCE(@MaxAbs_Difference, 0) > @Tol THEN N'WARN'
			WHEN COALESCE(@MaxAbs_LossCF_Delta_Error, 0) > @Tol THEN N'FAIL'
			WHEN COALESCE(@LossCF_Negative_Count, 0) > 0 THEN N'FAIL'
			ELSE N'PASS'
		END
	);

	FETCH NEXT FROM cur INTO @ScenarioId, @ScenarioName, @IsVatRegistered, @PriceRatio;
END

CLOSE cur;
DEALLOCATE cur;

SELECT *
FROM #ProofSummary
ORDER BY ScenarioId;
