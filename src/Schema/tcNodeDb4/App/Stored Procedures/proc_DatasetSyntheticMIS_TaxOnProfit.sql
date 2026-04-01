CREATE PROCEDURE App.proc_DatasetSyntheticMIS_TaxOnProfit
(
	@IsCompany bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @IsCompany = 0
		RETURN;

	DECLARE @CurrentPeriodStartOn date =
	(
		SELECT MIN(CAST(StartOn AS date))
		FROM App.tbYearPeriod
		WHERE CashStatusCode = 1
	);

	IF @CurrentPeriodStartOn IS NULL
		THROW 51360, 'DatasetSyntheticMIS_TaxOnProfit: unable to resolve current period.', 1;

	DECLARE
		@HmrcSubjectCode nvarchar(10),
		@BizTaxCashCode nvarchar(50);

	SELECT
		@HmrcSubjectCode = SubjectCode,
		@BizTaxCashCode = CashCode
	FROM Cash.tbTaxType
	WHERE TaxTypeCode = 0;

	IF @HmrcSubjectCode IS NULL OR @BizTaxCashCode IS NULL
		THROW 51361, 'DatasetSyntheticMIS_TaxOnProfit: Cash.tbTaxType missing SubjectCode/CashCode for TaxTypeCode=0.', 1;

	DECLARE
		@HomeSubjectCode nvarchar(10) = (SELECT SubjectCode FROM App.vwHomeAccount),
		@CurrentAccountCode nvarchar(10) = (SELECT AccountCode FROM Cash.vwCurrentAccount),
		@ReserveAccountCode nvarchar(10) = (SELECT AccountCode FROM Cash.vwReserveAccount),
		@UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials);

	IF @HomeSubjectCode IS NULL OR @CurrentAccountCode IS NULL OR @ReserveAccountCode IS NULL
		THROW 51362, 'DatasetSyntheticMIS_TaxOnProfit: missing Home/Current/Reserve account lookup.', 1;

	DECLARE
		@PayOn date = (SELECT MIN(CAST(StartOn AS date)) FROM App.tbYearPeriod),
		@Balance decimal(18,5),
		@PaymentCode nvarchar(20),
		@LoopGuard int = 0;

	WHILE 1 = 1
	BEGIN
		SET @LoopGuard += 1;
		IF @LoopGuard > 50
			THROW 51363, 'DatasetSyntheticMIS_TaxOnProfit: loop guard triggered.', 1;

		-- Move the search window forward so we don't re-read the payment rows we create
		SET @PayOn = DATEADD(month, 1, @PayOn);

		SELECT TOP (1)
			@PayOn = CAST(StartOn AS date),
			@Balance = Balance
		FROM Cash.vwTaxBizStatement
		WHERE CAST(StartOn AS date) BETWEEN @PayOn AND @CurrentPeriodStartOn
		  AND Balance > 0
		ORDER BY StartOn;

		IF @Balance IS NULL OR @Balance <= 0
			BREAK;

        -----------------------------------------------------------------
		-- 1) Transfer required funds Reserve -> Current
		-----------------------------------------------------------------
		EXEC App.proc_DatasetSyntheticMIS_Transfer
			@PaidOn = @PayOn,
			@Amount = @Balance,
			@FromAccountCode = @ReserveAccountCode,
			@ToAccountCode = @CurrentAccountCode;

		-----------------------------------------------------------------
		-- 2) Pay HMRC from current account (TC602), then post
		-----------------------------------------------------------------
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
			2,
			@HmrcSubjectCode,
			@CurrentAccountCode,
			@BizTaxCashCode,
			N'N/A',
			@PayOn,
			0.00000,
			@Balance,
			N'DS BIZ TAX PAYMENT'
		);

		EXEC Cash.proc_PayAccrual @PaymentCode = @PaymentCode;

		-- reset for next iteration
		SET @Balance = NULL;
	END
GO
