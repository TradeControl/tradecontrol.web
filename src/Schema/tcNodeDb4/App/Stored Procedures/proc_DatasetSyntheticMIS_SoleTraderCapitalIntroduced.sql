CREATE PROCEDURE App.proc_DatasetSyntheticMIS_SoleTraderCapitalIntroduced
(
	@IsCompany bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @IsCompany <> 0
		RETURN;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51280, 'DatasetSyntheticMIS_SoleTraderCapitalIntroduced: missing temp table #DatasetCodes. Run via App.proc_DatasetSyntheticMIS.', 1;

	DECLARE @UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials);

	DECLARE @Year1 smallint = (SELECT MIN(YearNumber) FROM App.tbYear);

	IF @Year1 IS NULL
		THROW 51281, 'DatasetSyntheticMIS_SoleTraderCapitalIntroduced: App.tbYear is empty.', 1;

	DECLARE @Year1FirstStartOn date =
	(
		SELECT MIN(CAST(yp.StartOn AS date))
		FROM App.tbYearPeriod yp
		WHERE yp.YearNumber = @Year1
	);

	IF @Year1FirstStartOn IS NULL
		THROW 51282, 'DatasetSyntheticMIS_SoleTraderCapitalIntroduced: unable to resolve year 1 start period.', 1;

	DECLARE @SettlementAccountCode nvarchar(10) =
		(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'SettlementAccountCode');

	IF @SettlementAccountCode IS NULL
		SELECT @SettlementAccountCode = AccountCode FROM Cash.vwCurrentAccount;

	IF @SettlementAccountCode IS NULL
		THROW 51283, 'DatasetSyntheticMIS_SoleTraderCapitalIntroduced: unable to resolve SettlementAccountCode.', 1;

	DECLARE @CapitalIntroduced decimal(18,5) = 5000.00000;
	DECLARE @PaidOn date = DATEADD(DAY, 10, @Year1FirstStartOn);

	-- Bank-side: owner introduces cash (what the trader sees)
	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = @SettlementAccountCode
		  AND p.CashCode = N'CAPIN01'
		  AND CAST(p.PaidOn AS date) = @PaidOn
		  AND p.PaymentReference = N'Owner Capital Introduced'
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
			N'CAPIN01',
			N'N/A',
			@PaidOn,
			@CapitalIntroduced,
			0.00000,
			N'Owner Capital Introduced'
		);
	END

	EXEC Cash.proc_PaymentPost;
GO
