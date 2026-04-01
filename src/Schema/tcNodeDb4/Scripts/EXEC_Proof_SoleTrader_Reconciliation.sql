SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @GenerateDataset bit = 1;            -- 1 = run App.proc_DatasetSyntheticMIS (slow)
DECLARE @EnableBankLedgerCrossCheck bit = 0; -- optional; not part of the capital proof
DECLARE @EnableVatProof bit = 1;             -- HMRC-facing proof (VAT statement by year)
DECLARE @InjectFault bit = 0;                -- set to 1 to force a FAIL
DECLARE @FaultAmount decimal(18, 5) = 0.11;  -- not used by this injector; kept for consistency

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
	,(N'VAT=1, PriceRatio=0.5 (loss)',   1, 0.5)
	, (N'VAT=0, PriceRatio=3.0 (profit)', 0, 3.0)
	, (N'VAT=1, PriceRatio=3.0 (profit)', 1, 3.0)

	;

IF OBJECT_ID('tempdb..#ProofSummary') IS NOT NULL DROP TABLE #ProofSummary;
CREATE TABLE #ProofSummary
(
	ScenarioId int NOT NULL,
	ScenarioName nvarchar(100) NOT NULL,
	ProofName nvarchar(200) NOT NULL,
	Tolerance decimal(18,4) NOT NULL,
	YearCount int NOT NULL,

	CapitalDelta_Definition_MaxAbsError decimal(38,18) NULL,
	Difference_Definition_MaxAbsError decimal(38,18) NULL,
	Difference_MaxAbs decimal(38,18) NULL,

	VatBalance_MaxAbs decimal(38,18) NULL,

	BankBalance_Definition_MaxAbsError decimal(38,18) NULL,
	BankBalance_MaxAbs decimal(38,18) NULL,

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
	PRINT CONCAT('Running scenario ', @ScenarioId, ': ', @ScenarioName, ' @InjectFault=', @InjectFault);

	IF @GenerateDataset = 1
	BEGIN
		EXEC App.proc_DatasetSyntheticMIS
			@IsCompany = 0,
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
			@EnableWages = 0,
			@EnableExpenses = 1,
			@EnableAssets = 0,
			@EnableTax = 1,
			@EnableTransfers = 1,
			@EnableOpeningBalance = 1;
	END

	---------------------------------------------------------------------
	-- Year ends: CLOSED years only (exclude current/incomplete year)
	---------------------------------------------------------------------
	IF OBJECT_ID('tempdb..#YearEnds') IS NOT NULL DROP TABLE #YearEnds;
	SELECT
		yp.YearNumber,
		YearEndOn = MAX(yp.StartOn)
	INTO #YearEnds
	FROM App.tbYearPeriod yp
	JOIN App.tbYear y ON yp.YearNumber = y.YearNumber
	WHERE y.CashStatusCode = 2
	GROUP BY yp.YearNumber;

	---------------------------------------------------------------------
	-- Proof 1: Sole trader capital bridge residual (vwSoleTraderReconciliationByYear)
	---------------------------------------------------------------------
	IF OBJECT_ID('tempdb..#Calc') IS NOT NULL DROP TABLE #Calc;
	SELECT
		CapitalDelta_Error = (r.CapitalDelta - (r.ClosingCapital - r.OpeningCapital)),
		Difference_Value = r.Difference
	INTO #Calc
	FROM Cash.vwSoleTraderReconciliationByYear r
	JOIN #YearEnds ye ON ye.YearNumber = r.YearNumber;

	---------------------------------------------------------------------
	-- Proof 2 (optional): VAT control (Balance Sheet) vs VAT statement
	--   VAT control: Cash.vwBalanceSheetVat at accounting snapshot StartOn = YearEndOn
	--   VAT statement: Cash.vwTaxVatStatement, take period-end balance rows (max RowNumber per StartOn)
	--                 then choose latest StartOn <= YearEndOn (as-of year end)
	---------------------------------------------------------------------
	IF OBJECT_ID('tempdb..#VatByYear') IS NOT NULL DROP TABLE #VatByYear;
	CREATE TABLE #VatByYear
	(
		YearNumber int NOT NULL,
		YearEndOn datetime NOT NULL,
		VatPayOn datetime NULL,
		VatStatementBalance decimal(18, 5) NOT NULL,
		VatControlBalance decimal(18, 5) NOT NULL,
		Difference decimal(18, 5) NOT NULL
	);

	IF @EnableVatProof = 1 AND @IsVatRegistered = 1
	BEGIN
		;WITH vat_due_dates AS
		(
			SELECT PayOn, PayFrom, PayTo
			FROM Cash.fnTaxTypeDueDates(1, 0)
		),
		vat_year_end AS
		(
			SELECT
				ye.YearNumber,
				ye.YearEndOn,
				VatPayOn =
					(
						SELECT TOP (1) dd.PayOn
						FROM vat_due_dates dd
						WHERE ye.YearEndOn >= dd.PayFrom
						  AND ye.YearEndOn < dd.PayTo
						ORDER BY dd.PayOn
					)
			FROM #YearEnds ye
		),
		vat_control AS
		(
			SELECT
				ye.YearNumber,
				VatControlBalance = SUM(COALESCE(vat.Balance, 0))
			FROM #YearEnds ye
			LEFT JOIN Cash.vwBalanceSheetVat vat
				ON vat.StartOn = ye.YearEndOn
			GROUP BY ye.YearNumber
		),
		vat_due_asof AS
		(
			SELECT
				ye.YearNumber,
				VatDue = SUM(COALESCE(vs.VatDue, 0))
			FROM #YearEnds ye
			LEFT JOIN Cash.vwTaxVatSummary vs
				ON vs.StartOn <= ye.YearEndOn
			GROUP BY ye.YearNumber
		),
		vat_paid_asof AS
		(
			SELECT
				ye.YearNumber,
				VatPaid = SUM(COALESCE(p.VatPaid, 0))
			FROM #YearEnds ye
			LEFT JOIN
			(
				SELECT
					yp.StartOn,
					VatPaid = (pay.PaidOutValue * -1) + pay.PaidInValue
				FROM Cash.tbPayment pay
				JOIN Cash.tbTaxType tt
					ON pay.CashCode = tt.CashCode
				CROSS APPLY
				(
					SELECT TOP (1) yp.StartOn
					FROM App.tbYearPeriod yp
					WHERE yp.StartOn <= pay.PaidOn
					ORDER BY yp.StartOn DESC
				) yp
				WHERE tt.TaxTypeCode = 1
			) p
				ON p.StartOn <= ye.YearEndOn
			GROUP BY ye.YearNumber
		)
		INSERT INTO #VatByYear
		(
			YearNumber,
			YearEndOn,
			VatPayOn,
			VatStatementBalance,
			VatControlBalance,
			Difference
		)
		SELECT
			vye.YearNumber,
			vye.YearEndOn,
			vye.VatPayOn,
			VatStatementBalance =
				CAST((COALESCE(vda.VatDue, 0) + COALESCE(vpa.VatPaid, 0)) * -1 AS decimal(18, 5)),
			VatControlBalance = CAST(COALESCE(vc.VatControlBalance, 0) AS decimal(18, 5)),
			Difference =
				CAST(
					COALESCE(vc.VatControlBalance, 0)
					- ((COALESCE(vda.VatDue, 0) + COALESCE(vpa.VatPaid, 0)) * -1)
					AS decimal(18, 5)
				)
		FROM vat_year_end vye
		LEFT JOIN vat_control vc
			ON vc.YearNumber = vye.YearNumber
		LEFT JOIN vat_due_asof vda
			ON vda.YearNumber = vye.YearNumber
		LEFT JOIN vat_paid_asof vpa
			ON vpa.YearNumber = vye.YearNumber;

		SELECT 'VAT by closed accounting year (invoice VAT due minus VAT paid, AS-OF YearEndOn):', *
		FROM #VatByYear
		ORDER BY YearNumber;
	END

	---------------------------------------------------------------------
	-- Optional bank ledger cross-check (kept, but does not affect PASS unless enabled)
	---------------------------------------------------------------------
	IF OBJECT_ID('tempdb..#BankBalDiff') IS NOT NULL DROP TABLE #BankBalDiff;

	WITH bank_accounts AS
	(
		SELECT AccountCode = (SELECT AccountCode FROM Cash.vwCurrentAccount)
		UNION ALL
		SELECT AccountCode = (SELECT AccountCode FROM Cash.vwReserveAccount)
	),
	last_stmt_per_account AS
	(
		SELECT
			ye.YearNumber,
			ba.AccountCode,
			LastPaidBalance = COALESCE(last_stmt.PaidBalance, 0)
		FROM #YearEnds ye
		CROSS JOIN bank_accounts ba
		OUTER APPLY
		(
			SELECT TOP (1) s.PaidBalance
			FROM Cash.vwAccountStatement s
			WHERE s.AccountCode = ba.AccountCode
			  AND s.StartOn = ye.YearEndOn
			ORDER BY s.PaidOn DESC, s.EntryNumber DESC
		) last_stmt
	),
	stmt_year_end AS
	(
		SELECT
			YearNumber,
			StatementBalance = SUM(LastPaidBalance)
		FROM last_stmt_per_account
		GROUP BY YearNumber
	)
	SELECT
		sye.YearNumber,
		sye.StatementBalance,
		ViewClosingCashAtBank = r.ClosingCashAtBank,
		Difference = CAST(sye.StatementBalance - r.ClosingCashAtBank AS decimal(18,5))
	INTO #BankBalDiff
	FROM stmt_year_end sye
	JOIN Cash.vwSoleTraderReconciliationByYear r
		ON r.YearNumber = sye.YearNumber;

	---------------------------------------------------------------------
	-- Summary stats + status
	---------------------------------------------------------------------
	DECLARE
		@Tol decimal(18,4) = 0.10,
		@YearCount int,
		@MaxAbs_CapitalDelta_Error decimal(38,18),
		@MaxAbs_Difference decimal(38,18),
		@MaxAbs_VatBalance decimal(38,18),
		@MaxAbs_BankBalance_Diff decimal(38,18);

	SELECT
		@YearCount = (SELECT COUNT(*) FROM #Calc),
		@MaxAbs_CapitalDelta_Error = (SELECT MAX(ABS(CapitalDelta_Error)) FROM #Calc),
		@MaxAbs_Difference = (SELECT MAX(ABS(Difference_Value)) FROM #Calc),
		@MaxAbs_VatBalance =
			CASE
				WHEN @EnableVatProof = 1 AND @IsVatRegistered = 1 THEN (SELECT MAX(ABS(Difference)) FROM #VatByYear)
				ELSE NULL
			END,
		@MaxAbs_BankBalance_Diff =
			CASE
				WHEN @EnableBankLedgerCrossCheck = 1 THEN (SELECT MAX(ABS(Difference)) FROM #BankBalDiff)
				ELSE NULL
			END;

	INSERT INTO #ProofSummary
	(
		ScenarioId,
		ScenarioName,
		ProofName,
		Tolerance,
		YearCount,
		CapitalDelta_Definition_MaxAbsError,
		Difference_Definition_MaxAbsError,
		Difference_MaxAbs,
		VatBalance_MaxAbs,
		BankBalance_Definition_MaxAbsError,
		BankBalance_MaxAbs,
		Status
	)
	VALUES
	(
		@ScenarioId,
		@ScenarioName,
		N'Sole Trader / Reconciliation Proofs',
		@Tol,
		COALESCE(@YearCount, 0),
		@MaxAbs_CapitalDelta_Error,
		@MaxAbs_Difference,
		@MaxAbs_Difference,
		@MaxAbs_VatBalance,
		CASE WHEN @EnableBankLedgerCrossCheck = 1 THEN @MaxAbs_BankBalance_Diff ELSE NULL END,
		CASE WHEN @EnableBankLedgerCrossCheck = 1 THEN @MaxAbs_BankBalance_Diff ELSE NULL END,
		CASE
			WHEN COALESCE(@MaxAbs_CapitalDelta_Error, 0) > @Tol THEN N'FAIL'
			WHEN COALESCE(@MaxAbs_Difference, 0) > @Tol THEN N'FAIL'
			WHEN @EnableVatProof = 1 AND @IsVatRegistered = 1 AND COALESCE(@MaxAbs_VatBalance, 0) > @Tol THEN N'FAIL'
			WHEN @EnableBankLedgerCrossCheck = 1 AND COALESCE(@MaxAbs_BankBalance_Diff, 0) > @Tol THEN N'FAIL'
			ELSE N'PASS'
		END
	);

	IF @EnableVatProof = 1 AND @IsVatRegistered = 1 AND COALESCE(@MaxAbs_VatBalance, 0) > @Tol
	BEGIN
		PRINT 'VAT balance at year-end (vwTaxVatStatement) exceeds tolerance:';
		SELECT *
		FROM #VatByYear
		ORDER BY YearNumber;
	END

	IF @EnableBankLedgerCrossCheck = 1 AND COALESCE(@MaxAbs_BankBalance_Diff, 0) > @Tol
	BEGIN
		PRINT 'Bank balance mismatch detected (vwAccountStatement vs vwSoleTraderReconciliationByYear.ClosingCashAtBank):';
		SELECT *
		FROM #BankBalDiff
		ORDER BY YearNumber;
	END

	FETCH NEXT FROM cur INTO @ScenarioId, @ScenarioName, @IsVatRegistered, @PriceRatio;
END

CLOSE cur;
DEALLOCATE cur;

SELECT
	ScenarioId,
	ScenarioName,
	ProofName,
	Tolerance,
	YearCount,
	CapitalDelta_Definition_MaxAbsError,
	Difference_Definition_MaxAbsError,
	Difference_MaxAbs,
	VatBalance_MaxAbs,
	BankBalance_Definition_MaxAbsError,
	BankBalance_MaxAbs,
	Status
FROM #ProofSummary
ORDER BY ScenarioId;
