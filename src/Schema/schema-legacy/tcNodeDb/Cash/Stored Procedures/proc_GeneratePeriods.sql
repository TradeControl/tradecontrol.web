CREATE PROCEDURE Cash.proc_GeneratePeriods
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@YearNumber smallint
		, @StartOn datetime
		, @PeriodStartOn datetime
		, @CashStatusCode smallint
		, @Period smallint
	
		DECLARE curYr cursor for	
			SELECT     YearNumber, CAST(CONCAT(FORMAT(YearNumber, '0000'), FORMAT(StartMonth, '00'), FORMAT(1, '00')) AS DATE) AS StartOn, CashStatusCode
			FROM         App.tbYear
			WHERE CashStatusCode < 2

		OPEN curYr
	
		FETCH NEXT FROM curYr INTO @YearNumber, @StartOn, @CashStatusCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			SET @PeriodStartOn = @StartOn
			SET @Period = 1
			WHILE @Period < 13
				BEGIN
				IF not EXISTS (SELECT MonthNumber FROM App.tbYearPeriod WHERE YearNumber = @YearNumber and MonthNumber = DATEPART(m, @PeriodStartOn))
					BEGIN
					INSERT INTO App.tbYearPeriod (YearNumber, StartOn, MonthNumber, CashStatusCode)
					VALUES (@YearNumber, @PeriodStartOn, DATEPART(m, @PeriodStartOn), 0)				
					END
				SET @PeriodStartOn = DATEADD(m, 1, @PeriodStartOn)	
				SET @Period = @Period + 1
				END		
				
			FETCH NEXT FROM curYr INTO @YearNumber, @StartOn, @CashStatusCode
			END
	
		CLOSE curYr
		DEALLOCATE curYr
	
		INSERT INTO Cash.tbPeriod
							  (CashCode, StartOn)
		SELECT     Cash.vwPeriods.CashCode, Cash.vwPeriods.StartOn
		FROM         Cash.vwPeriods LEFT OUTER JOIN
							  Cash.tbPeriod ON Cash.vwPeriods.CashCode = Cash.tbPeriod.CashCode AND Cash.vwPeriods.StartOn = Cash.tbPeriod.StartOn
		WHERE     ( Cash.tbPeriod.CashCode IS NULL)
		 
		UPDATE Cash.tbPeriod
		SET InvoiceValue = 0, InvoiceTax = 0;
		
		WITH invoice_entries AS
		(
			SELECT invoices.CashCode, invoices.StartOn, categories.CashModeCode, SUM(invoices.InvoiceValue) InvoiceValue, SUM(invoices.TaxValue) TaxValue
			FROM  Invoice.vwRegisterDetail invoices
				JOIN Cash.tbCode cash_codes ON invoices.CashCode = cash_codes.CashCode 
				JOIN Cash.tbCategory categories ON cash_codes.CategoryCode = categories .CategoryCode
			WHERE   (StartOn < (SELECT StartOn FROM App.fnActivePeriod()))
			GROUP BY invoices.CashCode, invoices.StartOn, categories.CashModeCode
		), invoice_summary AS
		(
			SELECT CashCode, StartOn,
				CASE CashModeCode 
					WHEN 0 THEN
						InvoiceValue * -1
					ELSE 
						InvoiceValue
				END AS InvoiceValue,
				CASE CashModeCode 
					WHEN 0 THEN
						TaxValue * -1
					ELSE 
						TaxValue
				END AS TaxValue						
			FROM invoice_entries
		)
		UPDATE Cash.tbPeriod
		SET InvoiceValue = invoice_summary.InvoiceValue, 
			InvoiceTax = invoice_summary.TaxValue
		FROM    Cash.tbPeriod 
			JOIN invoice_summary 
				ON Cash.tbPeriod.CashCode = invoice_summary.CashCode AND Cash.tbPeriod.StartOn = invoice_summary.StartOn;

		WITH asset_entries AS
		(
			SELECT payment.CashCode, 
				(SELECT TOP 1 StartOn FROM App.tbYearPeriod WHERE (StartOn <= payment.PaidOn) ORDER BY StartOn DESC) AS StartOn,
				CASE cash_category.CashModeCode
					WHEN 0 THEN (PaidInValue + (PaidOutValue * -1)) * -1
					WHEN 1 THEN PaidInValue + (PaidOutValue * -1)
				END AssetValue
			FROM Cash.tbPayment payment
				JOIN Org.tbAccount account ON payment.CashAccountCode = account.CashAccountCode
				JOIN Cash.tbCode cash_code ON account.CashCode = cash_code.CashCode
				JOIN Cash.tbCategory cash_category ON cash_code.CategoryCode = cash_category.CategoryCode
			WHERE account.AccountTypeCode = 2 AND payment.IsProfitAndLoss <> 0 AND PaidOn < (SELECT StartOn FROM App.fnActivePeriod())
		), asset_summary AS
		(
			SELECT CashCode, StartOn, SUM(AssetValue) AssetValue
			FROM asset_entries
			GROUP BY CashCode, StartOn
		)
		UPDATE Cash.tbPeriod
		SET InvoiceValue = AssetValue
		FROM  Cash.tbPeriod 
			JOIN asset_summary 
				ON Cash.tbPeriod.CashCode = asset_summary.CashCode AND Cash.tbPeriod.StartOn = asset_summary.StartOn;	

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
