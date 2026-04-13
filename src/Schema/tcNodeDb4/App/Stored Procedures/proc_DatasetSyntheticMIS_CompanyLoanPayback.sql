CREATE PROCEDURE App.proc_DatasetSyntheticMIS_CompanyLoanPayback
(
	@IsCompany bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @IsCompany = 0
		RETURN;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51296, 'DatasetSyntheticMIS_CompanyLoanPayback: missing temp table #DatasetCodes. Run via App.proc_DatasetSyntheticMIS.', 1;

	DECLARE @UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials);

	DECLARE @Year1 smallint = (SELECT MIN(YearNumber) FROM App.tbYear);

	IF @Year1 IS NULL
		THROW 51297, 'DatasetSyntheticMIS_CompanyLoanPayback: App.tbYear is empty.', 1;

	DECLARE @Year1FirstStartOn date =
	(
		SELECT MIN(CAST(yp.StartOn AS date))
		FROM App.tbYearPeriod yp
		WHERE yp.YearNumber = @Year1
	);

	IF @Year1FirstStartOn IS NULL
		THROW 51298, 'DatasetSyntheticMIS_CompanyLoanPayback: unable to resolve year 1 start period.', 1;

	DECLARE @SettlementAccountCode nvarchar(10) =
		(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'SettlementAccountCode');

	IF @SettlementAccountCode IS NULL
		SELECT @SettlementAccountCode = AccountCode FROM Cash.vwCurrentAccount;

	IF @SettlementAccountCode IS NULL
		THROW 51299, 'DatasetSyntheticMIS_CompanyLoanPayback: unable to resolve SettlementAccountCode.', 1;

	-- Director loan liability account (created by base template)
	DECLARE @LoanLiabilityAccountCode nvarchar(10) = N'LONLIA';

	IF NOT EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = @LoanLiabilityAccountCode AND AccountClosed = 0)
		THROW 51300, 'DatasetSyntheticMIS_CompanyLoanPayback: unable to resolve loan liability AccountCode (LONLIA).', 1;

	-- Cash code used on the loan liability account
	DECLARE @LoanCashCode nvarchar(50) =
	(
		SELECT CashCode
		FROM Subject.tbAccount
		WHERE AccountCode = @LoanLiabilityAccountCode
	);

	IF @LoanCashCode IS NULL
		SET @LoanCashCode = N'CC-LOAN';

	---------------------------------------------------------------------
	-- Add payback events (polarity-driven on CC-LOAN)
	-- These are posted against the loan liability account (AssetTypeCode=2).
	-- Positive vs negative is expressed by PaidInValue/PaidOutValue.
	---------------------------------------------------------------------
	DECLARE @Payback1 decimal(18,5) = 1200.00000;
	DECLARE @Payback2 decimal(18,5) = 800.00000;

	DECLARE @PaidOn1 date = DATEADD(DAY, 55, @Year1FirstStartOn);
	DECLARE @PaidOn2 date = DATEADD(DAY, 95, @Year1FirstStartOn);

	-- Event 1: bank payment out (cash leaves settlement account)
	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @SettlementAccountCode
		  AND p.CashCode = @LoanCashCode
		  AND CAST(p.PaidOn AS date) = @PaidOn1
		  AND p.PaymentReference = N'Director Loan Payback'
	)
	BEGIN
		DECLARE @PayCode1 nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @PayCode1 OUTPUT;

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
			@PayCode1,
			@UserId,
			1,
			N'HOME',
			@SettlementAccountCode,
			@LoanCashCode,
			N'N/A',
			@PaidOn1,
			0.00000,
			@Payback1,
			N'Director Loan Payback'
		);
	END

	-- Event 1: loan liability leg (reduce liability)
	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @LoanLiabilityAccountCode
		  AND p.CashCode = @LoanCashCode
		  AND CAST(p.PaidOn AS date) = @PaidOn1
		  AND p.PaymentReference = N'Director Loan Payback'
	)
	BEGIN
		DECLARE @PayCode2 nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @PayCode2 OUTPUT;

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
			@PayCode2,
			@UserId,
			1,
			N'HOME',
			@LoanLiabilityAccountCode,
			@LoanCashCode,
			N'N/A',
			@PaidOn1,
			@Payback1,
			0.00000,
			N'Director Loan Payback'
		);
	END

	-- Event 2: bank payment out
	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @SettlementAccountCode
		  AND p.CashCode = @LoanCashCode
		  AND CAST(p.PaidOn AS date) = @PaidOn2
		  AND p.PaymentReference = N'Director Loan Payback'
	)
	BEGIN
		DECLARE @PayCode3 nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @PayCode3 OUTPUT;

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
			@PayCode3,
			@UserId,
			1,
			N'HOME',
			@SettlementAccountCode,
			@LoanCashCode,
			N'N/A',
			@PaidOn2,
			0.00000,
			@Payback2,
			N'Director Loan Payback'
		);
	END

	-- Event 2: loan liability leg
	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @LoanLiabilityAccountCode
		  AND p.CashCode = @LoanCashCode
		  AND CAST(p.PaidOn AS date) = @PaidOn2
		  AND p.PaymentReference = N'Director Loan Payback'
	)
	BEGIN
		DECLARE @PayCode4 nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @PayCode4 OUTPUT;

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
			@PayCode4,
			@UserId,
			1,
			N'HOME',
			@LoanLiabilityAccountCode,
			@LoanCashCode,
			N'N/A',
			@PaidOn2,
			@Payback2,
			0.00000,
			N'Director Loan Payback'
		);
	END

	EXEC Cash.proc_PaymentPost;

GO
