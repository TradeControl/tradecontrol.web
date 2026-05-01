CREATE PROCEDURE App.proc_DatasetSyntheticMIS_PayMisc
(
	@IsCompany bit,
	@IsVatRegistered bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51215, 'DatasetSyntheticMIS_ProjectPay: missing temp table #DatasetCodes. Ensure ProjectTran ran.', 1;

	DECLARE @LastClosedStartOn date =
		TRY_CONVERT(date, (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'LastClosedStartOn'));

	DECLARE @SettlementAccountCode nvarchar(10) =
		(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'SettlementAccountCode');

	IF @LastClosedStartOn IS NULL OR @SettlementAccountCode IS NULL
		THROW 51222, 'DatasetSyntheticMIS_PayMisc: missing LINK/LastClosedStartOn or LINK/SettlementAccountCode.', 1;

	DECLARE
		@L2_UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials),
		@L2_WalkInSubjectCode nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MiscCustomer1'),
		@SellerRootCode nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'SellerRoot');

	IF @L2_WalkInSubjectCode IS NULL
		THROW 51101, 'SyntheticDataset Layer2: missing SUBJECT/MiscCustomer1 (walk-in) in #DatasetCodes.', 1;

	IF @SellerRootCode IS NULL
		THROW 51226, 'DatasetSyntheticMIS_PayMisc: missing SUBJECT/SellerRoot in #DatasetCodes.', 1;

	DECLARE @PayMiscTaxCode nvarchar(10) =
		CASE WHEN ISNULL(@IsVatRegistered, 0) <> 0 THEN N'T1' ELSE N'T0' END;

	DECLARE
		@L2_EnergySupplierCode nvarchar(50) = NULL,
		@L2_SupermarketSupplierCode nvarchar(50) = NULL;

	EXEC Subject.proc_AddNamespace
		@RootSubjectCode = @SellerRootCode,
		@SubjectName = N'Dataset Energy Supplier',
		@SubjectTypeCode = 0,
		@SubjectCode = @L2_EnergySupplierCode OUTPUT;

	UPDATE Subject.tbSubject
	SET
		SubjectStatusCode = 1,
		TaxCode = N'T1',
		PaymentTerms = N'30 days',
		ExpectedDays = 0,
		PaymentDays = 30,
		PayDaysFromMonthEnd = 0,
		PayBalance = 1
	WHERE SubjectCode = @L2_EnergySupplierCode;

	UPDATE Subject.tbVirtual
	SET EUJurisdiction = 0
	WHERE SubjectCode = @L2_EnergySupplierCode;

	EXEC Subject.proc_AddNamespace
		@RootSubjectCode = @SellerRootCode,
		@SubjectName = N'Dataset Supermarket',
		@SubjectTypeCode = 0,
		@SubjectCode = @L2_SupermarketSupplierCode OUTPUT;

	UPDATE Subject.tbSubject
	SET
		SubjectStatusCode = 1,
		TaxCode = N'T1',
		PaymentTerms = N'30 days',
		ExpectedDays = 0,
		PaymentDays = 30,
		PayDaysFromMonthEnd = 0,
		PayBalance = 1
	WHERE SubjectCode = @L2_SupermarketSupplierCode;

	UPDATE Subject.tbVirtual
	SET EUJurisdiction = 0
	WHERE SubjectCode = @L2_SupermarketSupplierCode;

	DECLARE
		@L2_MonthStart date,
		@L2_MonthEnd date,
		@L2_MonthIndex int,
		@L2_PaymentCode nvarchar(20),
		@L2_Amount decimal(18, 5);

	DECLARE curL2Months CURSOR LOCAL FAST_FORWARD FOR
		SELECT MonthStartOn, MonthEndOn, MonthIndex
		FROM App.fnDatasetMonths(@LastClosedStartOn)
		ORDER BY MonthStartOn;

	OPEN curL2Months;
	FETCH NEXT FROM curL2Months INTO @L2_MonthStart, @L2_MonthEnd, @L2_MonthIndex;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF (@L2_MonthIndex % 3) = 0
		BEGIN
			SET @L2_Amount = CAST(250 + (ABS(CHECKSUM(CONCAT(N'DS:L2:ENERGY:', @L2_MonthIndex))) % 750) AS decimal(18,5));

			SET @L2_PaymentCode = NULL;
			EXEC Cash.proc_NextPaymentCode @PaymentCode = @L2_PaymentCode OUTPUT;

			INSERT INTO Cash.tbPayment
			(
				PaymentCode, UserId, PaymentStatusCode,
				SubjectCode, AccountCode,
				CashCode, TaxCode,
				PaidOn, PaidInValue, PaidOutValue,
				PaymentReference
			)
			VALUES
			(
				@L2_PaymentCode, @L2_UserId, 0,
				@L2_EnergySupplierCode, @SettlementAccountCode,
				N'CC-ADMIN', @PayMiscTaxCode,
				@L2_MonthEnd, 0, @L2_Amount,
				N'Electricity Charge'
			);

			EXEC Cash.proc_PaymentPost;
		END

		SET @L2_Amount = CAST(60 + (ABS(CHECKSUM(CONCAT(N'DS:L2:PROV:', @L2_MonthIndex))) % 140) AS decimal(18,5));

		SET @L2_PaymentCode = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @L2_PaymentCode OUTPUT;

		INSERT INTO Cash.tbPayment
		(
			PaymentCode, UserId, PaymentStatusCode,
			SubjectCode, AccountCode,
			CashCode, TaxCode,
			PaidOn, PaidInValue, PaidOutValue,
			PaymentReference
		)
		VALUES
		(
			@L2_PaymentCode, @L2_UserId, 0,
			@L2_SupermarketSupplierCode, @SettlementAccountCode,
			N'CC-ADMIN', @PayMiscTaxCode,
			@L2_MonthEnd, 0, @L2_Amount,
			N'Provisions'
		);

		EXEC Cash.proc_PaymentPost;

		SET @L2_Amount = CAST(25 + (ABS(CHECKSUM(CONCAT(N'DS:L2:WALKIN:', @L2_MonthIndex))) % 125) AS decimal(18,5));

		SET @L2_PaymentCode = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @L2_PaymentCode OUTPUT;

		INSERT INTO Cash.tbPayment
		(
			PaymentCode, UserId, PaymentStatusCode,
			SubjectCode, AccountCode,
			CashCode, TaxCode,
			PaidOn, PaidInValue, PaidOutValue,
			PaymentReference
		)
		VALUES
		(
			@L2_PaymentCode, @L2_UserId, 0,
			@L2_WalkInSubjectCode, @SettlementAccountCode,
			N'CC-INCME', @PayMiscTaxCode,
			@L2_MonthEnd, @L2_Amount, 0,
			N'Widget Purchase'
		);

		EXEC Cash.proc_PaymentPost;

		FETCH NEXT FROM curL2Months INTO @L2_MonthStart, @L2_MonthEnd, @L2_MonthIndex;
	END

	CLOSE curL2Months;
	DEALLOCATE curL2Months;
GO
