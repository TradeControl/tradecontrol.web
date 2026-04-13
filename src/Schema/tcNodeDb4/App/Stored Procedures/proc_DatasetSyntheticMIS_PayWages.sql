CREATE PROCEDURE App.proc_DatasetSyntheticMIS_PayWages
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
		THROW 51231, 'DatasetSyntheticMIS_PayWages: missing LINK/LastClosedStartOn or LINK/SettlementAccountCode.', 1;

	DECLARE
		@L2_UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials),
		@L2_EmployeeSubjectCode nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'Employee');

	IF @L2_EmployeeSubjectCode IS NULL
		THROW 51100, 'SyntheticDataset Layer2: missing SUBJECT/Employee in #DatasetCodes.', 1;

	-----------------------------------------------------------------
	-- Loop closed months and create wages + NI
	-----------------------------------------------------------------
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
		-- Wages A
		SET @L2_Amount = CAST(1200 + (ABS(CHECKSUM(CONCAT(N'DS:L2:WAGEA:', @L2_MonthIndex))) % 250) AS decimal(18,5));

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
			@L2_EmployeeSubjectCode, @SettlementAccountCode,
			N'CC-WAGES', N'N/A',
			@L2_MonthEnd, 0, @L2_Amount,
			N'Wages'
		);

		EXEC Cash.proc_PaymentPost;

		-- Wages B
		SET @L2_Amount = CAST(950 + (ABS(CHECKSUM(CONCAT(N'DS:L2:WAGEB:', @L2_MonthIndex))) % 250) AS decimal(18,5));

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
			@L2_EmployeeSubjectCode, @SettlementAccountCode,
			N'CC-WAGES', N'N/A',
			@L2_MonthEnd, 0, @L2_Amount,
			N'Wages'
		);

		EXEC Cash.proc_PaymentPost;

		-- NI (10% of wages this month)
		DECLARE @L2_WagesMonthTotal decimal(18,5) =
		(
			SELECT SUM(p.PaidOutValue)
			FROM Cash.tbPayment p
			WHERE p.SubjectCode = @L2_EmployeeSubjectCode
				AND p.CashCode = N'CC-WAGES'
				AND CAST(p.PaidOn AS date) = @L2_MonthEnd
		);

		DECLARE @HMRC_NI_Account nvarchar(50), @HMRC_NI_CashCode nvarchar(50);
		SELECT @HMRC_NI_Account = SubjectCode, @HMRC_NI_CashCode = CashCode
		FROM Cash.tbTaxType
		WHERE TaxTypeCode = 2;

		SET @L2_Amount = CAST(ROUND(COALESCE(@L2_WagesMonthTotal, 0) * 0.10, 2) AS decimal(18,5));

		IF @L2_Amount > 0
		BEGIN
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
				@HMRC_NI_Account, @SettlementAccountCode,
				@HMRC_NI_CashCode, N'N/A',
				@L2_MonthEnd, 0, @L2_Amount,
				N'Employee NI'
			);

			EXEC Cash.proc_PaymentPost;
		END

		FETCH NEXT FROM curL2Months INTO @L2_MonthStart, @L2_MonthEnd, @L2_MonthIndex;
	END

	CLOSE curL2Months;
	DEALLOCATE curL2Months;

