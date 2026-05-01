CREATE PROCEDURE App.proc_DatasetSyntheticMIS_Assets
(
	@IsCompany bit,
	@IsVatRegistered bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @IsCompany = 0
		RETURN;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51260, 'DatasetSyntheticMIS_Assets: missing temp table #DatasetCodes. Run via App.proc_DatasetSyntheticMIS.', 1;

	DECLARE @UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials);

	DECLARE @Year1 smallint = (SELECT MIN(YearNumber) FROM App.tbYear);

	IF @Year1 IS NULL
		THROW 51261, 'DatasetSyntheticMIS_Assets: App.tbYear is empty.', 1;

	DECLARE @Year1FirstStartOn date =
	(
		SELECT MIN(CAST(yp.StartOn AS date))
		FROM App.tbYearPeriod yp
		WHERE yp.YearNumber = @Year1
	);

	IF @Year1FirstStartOn IS NULL
		THROW 51262, 'DatasetSyntheticMIS_Assets: unable to resolve year 1 start period.', 1;

	DECLARE @FirstTradingOn date = @Year1FirstStartOn;

	DECLARE @Year1LatePurchaseOn date =
	(
		SELECT TOP (1) CAST(yp.StartOn AS date)
		FROM App.tbYearPeriod yp
		WHERE yp.YearNumber = @Year1
		  AND yp.CashStatusCode = 2
		ORDER BY yp.StartOn DESC
	);

	IF @Year1LatePurchaseOn IS NULL
	BEGIN
		SELECT @Year1LatePurchaseOn = MAX(CAST(yp.StartOn AS date))
		FROM App.tbYearPeriod yp
		WHERE yp.YearNumber = @Year1;
	END

	UPDATE Subject.tbAccount
	SET AccountClosed = 0
	WHERE AccountTypeCode = 2
	  AND AccountClosed = 1;

	DECLARE @SettlementAccountCode nvarchar(10) =
		(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'SettlementAccountCode');

	IF @SettlementAccountCode IS NULL
	BEGIN
		SELECT @SettlementAccountCode = AccountCode FROM Cash.vwCurrentAccount;
	END

	IF @SettlementAccountCode IS NULL
		THROW 51263, 'DatasetSyntheticMIS_Assets: unable to resolve SettlementAccountCode.', 1;

	DECLARE
		@BusinessRootCode nvarchar(50) = (SELECT TOP (1) SubjectCode FROM App.tbOptions),
		@SellerRootCode nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'SellerRoot');

	IF @SellerRootCode IS NULL
		THROW 51267, 'DatasetSyntheticMIS_Assets: missing SUBJECT/SellerRoot in #DatasetCodes.', 1;

	IF EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = N'CALUP' AND AccountClosed = 0)
	BEGIN
		DECLARE @SharePaymentCode nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @SharePaymentCode OUTPUT;

		IF NOT EXISTS
		(
			SELECT 1
			FROM Cash.tbPayment p
			WHERE p.PaymentReference = N'Called Up Shares'
			  AND p.SubjectCode = N'HOME'
			  AND p.AccountCode = N'CALUP'
			  AND CAST(p.PaidOn AS date) = @FirstTradingOn
		)
		BEGIN
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
				@SharePaymentCode,
				@UserId,
				1,
				N'HOME',
				N'CALUP',
				N'CC-SHCAP',
				N'N/A',
				@FirstTradingOn,
				0.00000,
				1.00000,
				N'Called Up Shares'
			);
		END
	END

	DECLARE
		@SidsCode nvarchar(50) = NULL,
		@EmployeeName nvarchar(100) = N'Sid Jones';

	EXEC Subject.proc_AddNamespace
		@RootSubjectCode = @BusinessRootCode,
		@SubjectName = @EmployeeName,
		@SubjectTypeCode = 9,
		@SubjectCode = @SidsCode OUTPUT;

	UPDATE Subject.tbSubject
	SET
		SubjectStatusCode = 1,
		TaxCode = N'N/A',
		PaymentTerms = N'Immediate',
		ExpectedDays = 0,
		PaymentDays = 0,
		PayDaysFromMonthEnd = 0,
		PayBalance = 1
	WHERE SubjectCode = @SidsCode;

	IF NOT EXISTS
	(
		SELECT 1
		FROM Subject.tbSubject s
		JOIN Subject.tbAddress a ON a.AddressCode = s.AddressCode
		WHERE s.SubjectCode = @SidsCode
	)
	BEGIN
		DECLARE @Address nvarchar(max) = CONCAT(N'Residence of ', @EmployeeName);

		EXEC Subject.proc_AddAddress
			@SubjectCode = @SidsCode,
			@Address = @Address;
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT' AS CodeType, N'Director' AS CodeName, @SidsCode AS CodeValue, NULL AS RelatedName, N'Sid Jones' AS Notes) AS s
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, Notes = s.Notes;

	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = @SidsCode
		  AND p.AccountCode = @SettlementAccountCode
		  AND p.CashCode = N'CC-INCME'
		  AND CAST(p.PaidOn AS date) = @FirstTradingOn
		  AND p.PaymentReference = N'Share Capital Subscription'
	)
	BEGIN
		DECLARE @ShareInPaymentCode nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @ShareInPaymentCode OUTPUT;

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
			@ShareInPaymentCode,
			@UserId,
			0,
			@SidsCode,
			@SettlementAccountCode,
			N'CC-INCME',
			N'N/A',
			@FirstTradingOn,
			1.00000,
			0.00000,
			N'Share Capital Subscription'
		);
	END

	DECLARE @LoanAmount decimal(18,5) = 5000.00000;

	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = @SidsCode
		  AND p.AccountCode = @SettlementAccountCode
		  AND p.CashCode = N'CC-INCME'
		  AND CAST(p.PaidOn AS date) = @FirstTradingOn
		  AND p.PaymentReference = N'Director Loan Injection'
	)
	BEGIN
		DECLARE @InjectPaymentCode nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @InjectPaymentCode OUTPUT;

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
			@InjectPaymentCode,
			@UserId,
			0,
			@SidsCode,
			@SettlementAccountCode,
			N'CC-INCME',
			N'N/A',
			DATEADD(DAY, 7, @FirstTradingOn),
			@LoanAmount,
			0.00000,
			N'Director Loan Injection'
		);
	END

	IF EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = N'LONLIA' AND AccountClosed = 0)
	BEGIN
		IF NOT EXISTS
		(
			SELECT 1
			FROM Cash.tbPayment p
			WHERE p.SubjectCode = N'HOME'
			  AND p.AccountCode = N'LONLIA'
			  AND p.CashCode = N'CC-LOAN'
			  AND CAST(p.PaidOn AS date) = @FirstTradingOn
			  AND p.PaymentReference = N'Director Loan Liability'
		)
		BEGIN
			DECLARE @LiabPaymentCode nvarchar(20) = NULL;
			EXEC Cash.proc_NextPaymentCode @PaymentCode = @LiabPaymentCode OUTPUT;

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
				@LiabPaymentCode,
				@UserId,
				1,
				N'HOME',
				N'LONLIA',
				N'CC-LOAN',
				N'N/A',
				DATEADD(DAY, 7, @FirstTradingOn),
				0.00000,
				@LoanAmount,
				N'Director Loan Liability'
			);
		END
	END

	EXEC Cash.proc_PaymentPost;

	IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = N'CC-MREPA')
	BEGIN
		INSERT INTO Cash.tbCode
		(
			CashCode,
			CashDescription,
			CategoryCode,
			TaxCode,
			IsEnabled
		)
		VALUES
		(
			N'CC-MREPA',
			N'Motors',
			N'CA-DIRECT',
			N'T1',
			1
		);
	END

	DECLARE @GarageCode nvarchar(50) = NULL;

	EXEC Subject.proc_AddNamespace
		@RootSubjectCode = @SellerRootCode,
		@SubjectName = N'Dataset Garage',
		@SubjectTypeCode = 0,
		@SubjectCode = @GarageCode OUTPUT;

	IF @GarageCode IS NULL
		THROW 51265, 'DatasetSyntheticMIS_Assets: failed to allocate SubjectCode for Dataset Garage.', 1;

	UPDATE Subject.tbSubject
	SET
		SubjectStatusCode = 1,
		TaxCode = N'T1',
		PaymentTerms = N'30 days',
		ExpectedDays = 0,
		PaymentDays = 30,
		PayDaysFromMonthEnd = 0,
		PayBalance = 1
	WHERE SubjectCode = @GarageCode;

	UPDATE Subject.tbVirtual
	SET EUJurisdiction = 0
	WHERE SubjectCode = @GarageCode;

	IF NOT EXISTS
	(
		SELECT 1
		FROM Subject.tbSubject s
		JOIN Subject.tbAddress a ON a.AddressCode = s.AddressCode
		WHERE s.SubjectCode = @GarageCode
	)
	BEGIN
		EXEC Subject.proc_AddAddress
			@SubjectCode = @GarageCode,
			@Address = N'Address of Dataset Garage';
	END

	DECLARE @VehicleCost decimal(18,5) = 5000.00000;

	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = @GarageCode
		  AND p.CashCode = N'CC-MREPA'
		  AND p.PaymentReference = N'White Van'
		  AND CAST(p.PaidOn AS date) = @Year1LatePurchaseOn
	)
	BEGIN
		DECLARE @VehiclePayCode nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @VehiclePayCode OUTPUT;

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
			@VehiclePayCode,
			@UserId,
			0,
			@GarageCode,
			@SettlementAccountCode,
			N'CC-MREPA',
			N'T1',
			@Year1LatePurchaseOn,
			0.00000,
			@VehicleCost,
			N'White Van'
		);
	END

	EXEC Cash.proc_PaymentPost;

	DECLARE @EquipCashCode nvarchar(50) =
	(
		SELECT CashCode
		FROM Subject.tbAccount
		WHERE AccountTypeCode = 2
		  AND AccountCode = N'EQUIPM'
		  AND AccountClosed = 0
	);

	IF @EquipCashCode IS NULL
		THROW 51266, 'DatasetSyntheticMIS_Assets: EQUIPM account missing/closed (expected open for depreciation).', 1;

	IF NOT EXISTS
	(
		SELECT 1
		FROM Cash.tbPayment p
		WHERE p.SubjectCode = N'HOME'
		  AND p.AccountCode = N'EQUIPM'
		  AND p.PaymentReference = N'White Van (Capitalised)'
		  AND CAST(p.PaidOn AS date) = @Year1LatePurchaseOn
	)
	BEGIN
		DECLARE @VehicleCapCode nvarchar(20) = NULL;
		EXEC Cash.proc_NextPaymentCode @PaymentCode = @VehicleCapCode OUTPUT;

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
			@VehicleCapCode,
			@UserId,
			1,
			N'HOME',
			N'EQUIPM',
			@EquipCashCode,
			N'N/A',
			@Year1LatePurchaseOn,
			@VehicleCost,
			0.00000,
			N'White Van (Capitalised)'
		);
	END

	DECLARE
		@PurchaseOn date = @Year1LatePurchaseOn,
		@Price decimal(18,5) = @VehicleCost,
		@WriteOffPercent decimal(9,6) = 0.20;

	IF @WriteOffPercent <= 0 OR @WriteOffPercent > 1
		THROW 51270, 'DatasetSyntheticMIS_Assets: invalid @WriteOffPercent.', 1;

	DECLARE @Count int = CONVERT(int, ROUND(1.0 / @WriteOffPercent, 0));

	IF @Count <= 0
		THROW 51271, 'DatasetSyntheticMIS_Assets: invalid depreciation Count.', 1;

	DECLARE @PurchaseYear smallint =
	(
		SELECT TOP (1) yp.YearNumber
		FROM App.tbYearPeriod yp
		WHERE CAST(yp.StartOn AS date) <= @PurchaseOn
		ORDER BY yp.StartOn DESC
	);

	IF @PurchaseYear IS NULL
		THROW 51272, 'DatasetSyntheticMIS_Assets: unable to resolve purchase year.', 1;

	DECLARE @WriteOffDate date =
	(
		SELECT CAST(EOMONTH(MAX(CAST(yp.StartOn AS date))) AS date)
		FROM App.tbYearPeriod yp
		WHERE yp.YearNumber = @PurchaseYear
	);

	IF @WriteOffDate IS NULL
		THROW 51273, 'DatasetSyntheticMIS_Assets: unable to resolve WriteOffDate.', 1;

	WHILE @Count > 0
	BEGIN
		DECLARE @StatusCode smallint =
		(
			SELECT TOP (1) yp.CashStatusCode
			FROM App.tbYearPeriod yp
			WHERE CAST(yp.StartOn AS date) <= @WriteOffDate
			ORDER BY yp.StartOn DESC
		);

		DECLARE @PaymentStatusCode smallint = CASE WHEN @StatusCode = 2 THEN 1 ELSE 0 END;

		IF NOT EXISTS
		(
			SELECT 1
			FROM Cash.tbPayment p
			WHERE p.SubjectCode = N'HOME'
			  AND p.AccountCode = N'EQUIPM'
			  AND p.PaymentReference = N'Depreciation - White Van'
			  AND CAST(p.PaidOn AS date) = @WriteOffDate
		)
		BEGIN
			DECLARE @DepPaymentCode nvarchar(20) = NULL;
			EXEC Cash.proc_NextPaymentCode @PaymentCode = @DepPaymentCode OUTPUT;

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
				@DepPaymentCode,
				@UserId,
				@PaymentStatusCode,
				N'HOME',
				N'EQUIPM',
				@EquipCashCode,
				N'N/A',
				@WriteOffDate,
				0.00000,
				CAST(ROUND(@Price * @WriteOffPercent, 2) AS decimal(18,5)),
				N'Depreciation - White Van'
			);
		END

		SET @WriteOffDate = DATEADD(year, 1, @WriteOffDate);
		SET @Count -= 1;
	END
GO
