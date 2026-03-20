CREATE PROCEDURE App.proc_DatasetSyntheticMIS_TaxVat
(
	@IsCompany bit,
	@IsVatRegistered bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @IsVatRegistered = 0
		RETURN;

	DECLARE @SettlementAccountCode nvarchar(10) = (SELECT ca.AccountCode FROM Cash.vwCurrentAccount ca);

	DECLARE @CurrentPeriodStartOn date =
	(
		SELECT MIN(CAST(StartOn AS date))
		FROM App.tbYearPeriod
		WHERE CashStatusCode = 1
	);

	IF @CurrentPeriodStartOn IS NULL
		THROW 51292, 'DatasetSyntheticMIS_TaxVat: unable to resolve @CurrentPeriodStartOn from App.tbYearPeriod.', 1;

    ---------------------------------------------------------------------
	-- VAT adjustments (Admin Manager behavior):
	-- stored on App.tbYearPeriod.VatAdjustment in the last month of the quarter (PayTo)
	-- Apply only for quarters that are due (PayOn <= current period start).
	---------------------------------------------------------------------
	;WITH vat_dates AS
	(
		SELECT CAST(PayOn AS date) AS PayOn, CAST(PayTo AS date) AS PayTo
		FROM Cash.fnTaxTypeDueDates(1)
	),
	due_quarters AS
	(
		SELECT vd.PayTo
		FROM vat_dates vd
		WHERE vd.PayOn <= @CurrentPeriodStartOn
	),
	target_periods AS
	(
		SELECT yp.StartOn
		FROM App.tbYearPeriod yp
		JOIN due_quarters dq
			ON CAST(yp.StartOn AS date) = dq.PayTo
	)
	UPDATE yp
	SET VatAdjustment =
		CASE (ABS(CHECKSUM(CONCAT(N'DS:VATADJ:', CONVERT(nvarchar(10), CAST(yp.StartOn AS date), 23)))) % 3)
			WHEN 0 THEN 0.01
			WHEN 1 THEN -0.02
			ELSE 0.00
		END
	FROM App.tbYearPeriod yp
	JOIN target_periods t
		ON t.StartOn = yp.StartOn
	WHERE ISNULL(yp.VatAdjustment, 0) = 0
	  AND (ABS(CHECKSUM(CONCAT(N'DS:VATADJ:ENABLE:', CONVERT(nvarchar(10), CAST(yp.StartOn AS date), 23)))) % 4) <> 0;

    ---------------------------------------------------------------------
    -- Pay vat balance from Cash.vwVatStatement
    ---------------------------------------------------------------------
	DECLARE
		@HmrcSubjectCode nvarchar(10),
		@VatCashCode nvarchar(50);

	SELECT
		@HmrcSubjectCode = SubjectCode,
		@VatCashCode = CashCode
	FROM Cash.tbTaxType
	WHERE TaxTypeCode = 1;

	IF @HmrcSubjectCode IS NULL OR @VatCashCode IS NULL
		THROW 51293, 'DatasetSyntheticMIS_TaxVat: Cash.tbTaxType missing SubjectCode/CashCode for TaxTypeCode=1.', 1;

	DECLARE @UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials);

	DECLARE
		@PayOn date = (SELECT MIN(CAST(StartOn AS date)) FROM App.tbYearPeriod),
		@Balance decimal(18,5),
		@PaymentCode nvarchar(20),
		@PaidOut decimal(18,5),
		@PaidIn decimal(18,5),
		@LoopGuard int = 0;

	WHILE 1 = 1
	BEGIN
		SET @LoopGuard += 1;
		IF @LoopGuard > 200
			THROW 51294, 'DatasetSyntheticMIS_TaxVat: loop guard triggered (too many VAT payment iterations).', 1;

		-- Move the search window forward so we never re-read the last processed PayOn
		SET @PayOn = DATEADD(month, 1, @PayOn);

		SELECT TOP (1)
			@PayOn = CAST(StartOn AS date),
			@Balance = Balance
		FROM Cash.vwTaxVatStatement
		WHERE CAST(StartOn AS date) BETWEEN @PayOn AND @CurrentPeriodStartOn
		  AND Balance <> 0
		ORDER BY RowNumber;

		IF @Balance IS NULL
			BREAK;

		SET @PaidOut = CASE WHEN @Balance > 0 THEN @Balance ELSE CAST(0 AS decimal(18,5)) END;
		SET @PaidIn  = CASE WHEN @Balance < 0 THEN ABS(@Balance) ELSE CAST(0 AS decimal(18,5)) END;

		SET @PaymentCode = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @PaymentCode OUTPUT;

		INSERT INTO Cash.tbPayment
		(
			PaymentCode,
			UserId,
			PaymentStatusCode,
			SubjectCode,
			AccountCode,
			CashCode,
			TaxCode,
			PaidOn,
			PaidInValue,
			PaidOutValue,
			PaymentReference
		)
		VALUES
		(
			@PaymentCode,
			@UserId,
			0,
			@HmrcSubjectCode,
			@SettlementAccountCode,
			@VatCashCode,
			N'N/A',
			@PayOn,
			@PaidIn,
			@PaidOut,
			N'DS VAT PAYMENT'
		);

		EXEC Cash.proc_PaymentPost;

		-- reset for next iteration
		SET @Balance = NULL;
	END
