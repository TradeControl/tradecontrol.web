CREATE PROCEDURE App.proc_DatasetSyntheticMIS_SoleTraderDrawings
(
	@IsCompany bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @IsCompany <> 0
		RETURN;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51290, 'DatasetSyntheticMIS_SoleTraderDrawings: missing temp table #DatasetCodes. Run via App.proc_DatasetSyntheticMIS.', 1;

	DECLARE @UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials);

	DECLARE @Year1 smallint = (SELECT MIN(YearNumber) FROM App.tbYear);

	IF @Year1 IS NULL
		THROW 51291, 'DatasetSyntheticMIS_SoleTraderDrawings: App.tbYear is empty.', 1;

	DECLARE @Year1FirstStartOn date =
	(
		SELECT MIN(CAST(yp.StartOn AS date))
		FROM App.tbYearPeriod yp
		WHERE yp.YearNumber = @Year1
	);

	IF @Year1FirstStartOn IS NULL
		THROW 51292, 'DatasetSyntheticMIS_SoleTraderDrawings: unable to resolve year 1 start period.', 1;

	DECLARE @SettlementAccountCode nvarchar(10) =
		(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'SettlementAccountCode');

	IF @SettlementAccountCode IS NULL
		SELECT @SettlementAccountCode = AccountCode FROM Cash.vwCurrentAccount;

	IF @SettlementAccountCode IS NULL
		THROW 51293, 'DatasetSyntheticMIS_SoleTraderDrawings: unable to resolve SettlementAccountCode.', 1;

	DECLARE @OwnerCapitalAccountCode nvarchar(10) = N'OWNCAP';

	IF NOT EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = @OwnerCapitalAccountCode)
		THROW 51294, 'DatasetSyntheticMIS_SoleTraderDrawings: unable to resolve owner capital AccountCode (OWNCAP).', 1;

	DECLARE @CashCode nvarchar(50) = N'CC-OWNCAP';

	DECLARE @Drawing1 decimal(18,5) = 750.00000;
	DECLARE @Drawing2 decimal(18,5) = 900.00000;

	DECLARE @PaidOn1 date = DATEADD(DAY, 40, @Year1FirstStartOn);
	DECLARE @PaidOn2 date = DATEADD(DAY, 75, @Year1FirstStartOn);

	-- Event 1
	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @SettlementAccountCode
		  AND p.CashCode = @CashCode
		  AND CAST(p.PaidOn AS date) = @PaidOn1
		  AND p.PaymentReference = N'Owner Drawings'
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
			@CashCode,
			N'N/A',
			@PaidOn1,
			0.00000,
			@Drawing1,
			N'Owner Drawings'
		);
	END

	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @OwnerCapitalAccountCode
		  AND p.CashCode = @CashCode
		  AND CAST(p.PaidOn AS date) = @PaidOn1
		  AND p.PaymentReference = N'Owner Drawings'
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
			@OwnerCapitalAccountCode,
			@CashCode,
			N'N/A',
			@PaidOn1,
			@Drawing1,
			0.00000,
			N'Owner Drawings'
		);
	END

	-- Event 2
	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @SettlementAccountCode
		  AND p.CashCode = @CashCode
		  AND CAST(p.PaidOn AS date) = @PaidOn2
		  AND p.PaymentReference = N'Owner Drawings'
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
			@CashCode,
			N'N/A',
			@PaidOn2,
			0.00000,
			@Drawing2,
			N'Owner Drawings'
		);
	END

	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @OwnerCapitalAccountCode
		  AND p.CashCode = @CashCode
		  AND CAST(p.PaidOn AS date) = @PaidOn2
		  AND p.PaymentReference = N'Owner Drawings'
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
			@OwnerCapitalAccountCode,
			@CashCode,
			N'N/A',
			@PaidOn2,
			@Drawing2,
			0.00000,
			N'Owner Drawings'
		);
	END

	EXEC Cash.proc_PaymentPost;

GO
