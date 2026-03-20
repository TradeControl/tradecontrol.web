CREATE PROCEDURE App.proc_DatasetSyntheticMIS_ProjectPay
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
		THROW 51216, 'DatasetSyntheticMIS_ProjectPay: missing LINK/LastClosedStartOn or LINK/SettlementAccountCode.', 1;

	IF OBJECT_ID('tempdb..#Payables') IS NOT NULL DROP TABLE #Payables;

	SELECT DISTINCT i.SubjectCode
	INTO #Payables
	FROM Invoice.tbInvoice i;

	DECLARE
		@PayMonthStart date,
		@PayMonthEnd date,
		@PaySubjectCode nvarchar(10),
		@Balance float,
		@PaidOn date,
		@PaymentCode nvarchar(20),
		@PayPct decimal(9,6) = 0.80,
		@PayAmount decimal(18,5);

	DECLARE curPayMonths CURSOR LOCAL FAST_FORWARD FOR
		SELECT MonthStartOn, MonthEndOn
		FROM App.fnDatasetMonths(@LastClosedStartOn)
		ORDER BY MonthStartOn;

	OPEN curPayMonths;
	FETCH NEXT FROM curPayMonths INTO @PayMonthStart, @PayMonthEnd;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @PaidOn = @PayMonthEnd;

		DECLARE curSubjects CURSOR LOCAL FAST_FORWARD FOR
			SELECT SubjectCode
			FROM #Payables
			ORDER BY SubjectCode;

		OPEN curSubjects;
		FETCH NEXT FROM curSubjects INTO @PaySubjectCode;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT TOP (1) @Balance = COALESCE(Balance, 0)
			FROM Subject.vwStatement
			WHERE SubjectCode = @PaySubjectCode
				AND CAST(TransactedOn AS date) <= @PayMonthEnd
			ORDER BY TransactedOn DESC, RowNumber DESC;

			IF @Balance <> 0
			BEGIN
				SET @PayAmount =
					CAST(ROUND(ABS(CAST(@Balance AS decimal(18,5))) * @PayPct, 2) AS decimal(18,5));

				IF @PayAmount > 0
				BEGIN
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
					SELECT
						@PaymentCode,
						(SELECT UserId FROM Usr.vwCredentials),
						0,
						@PaySubjectCode,
						@SettlementAccountCode,
						NULL,
						NULL,
						@PaidOn,
						CASE WHEN @Balance < 0 THEN @PayAmount ELSE CAST(0 AS decimal(18,5)) END,
						CASE WHEN @Balance > 0 THEN @PayAmount ELSE CAST(0 AS decimal(18,5)) END,
						CONCAT(N'DS SETTLE ', FORMAT(@PaidOn, 'yyyy-MM'), N' ', CAST(@PayPct * 100 AS int), N'%');

					EXEC Cash.proc_PaymentPost;
				END
			END

			FETCH NEXT FROM curSubjects INTO @PaySubjectCode;
		END

		CLOSE curSubjects;
		DEALLOCATE curSubjects;

		FETCH NEXT FROM curPayMonths INTO @PayMonthStart, @PayMonthEnd;
	END

	CLOSE curPayMonths;
	DEALLOCATE curPayMonths;
