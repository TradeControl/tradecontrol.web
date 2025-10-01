CREATE   PROCEDURE Cash.proc_FlowCashCodeValues(@CashCode nvarchar(50), @YearNumber smallint, @IncludeActivePeriods BIT = 0, @IncludeOrderBook BIT = 0, @IncludeTaxAccruals BIT = 0)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @StartOn DATE
			, @IsTaxCode BIT = 0;

		DECLARE @tbReturn AS TABLE (
			StartOn DATETIME NOT NULL, 
			CashStatusCode SMALLINT NOT NULL, 
			ForecastValue DECIMAL(18, 5) NOT NULL, 
			ForecastTax DECIMAL(18, 5) NOT NULL, 
			InvoiceValue DECIMAL(18, 5) NOT NULL, 
			InvoiceTax DECIMAL(18, 5) NOT NULL);

		INSERT INTO @tbReturn (StartOn, CashStatusCode, ForecastValue, ForecastTax, InvoiceValue, InvoiceTax)
		SELECT   Cash.tbPeriod.StartOn, App.tbYearPeriod.CashStatusCode,
			Cash.tbPeriod.ForecastValue, 
			Cash.tbPeriod.ForecastTax, 
			CASE App.tbYearPeriod.CashStatusCode WHEN 2 THEN Cash.tbPeriod.InvoiceValue ELSE 0 END AS InvoiceValue, 
			CASE App.tbYearPeriod.CashStatusCode WHEN 2 THEN Cash.tbPeriod.InvoiceTax ELSE 0 END AS InvoiceTax
		FROM            Cash.tbPeriod INNER JOIN
									App.tbYearPeriod ON Cash.tbPeriod.StartOn = App.tbYearPeriod.StartOn INNER JOIN
									App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode < 3) AND (App.tbYearPeriod.YearNumber = @YearNumber) AND (Cash.tbPeriod.CashCode = @CashCode);

	
		SELECT @StartOn = (SELECT CAST(MIN(StartOn) AS DATE) FROM @tbReturn WHERE CashStatusCode < 2);
		IF EXISTS(SELECT * FROM Cash.tbTaxType tt WHERE tt.CashCode = @CashCode) SET @IsTaxCode = 1;

		IF (@IncludeActivePeriods <> 0)
			BEGIN		
			WITH active_candidates AS
			(
				SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod AS p WHERE (StartOn <= invoices.InvoicedOn) ORDER BY StartOn DESC) AS StartOn,
						CASE WHEN invoice_type.CashModeCode = 0 THEN tasks.InvoiceValue * - 1 ELSE tasks.InvoiceValue END AS InvoiceValue, 
						CASE WHEN invoice_type.CashModeCode = 0 THEN tasks.TaxValue * - 1 ELSE tasks.TaxValue END AS InvoiceTax
				FROM Invoice.tbInvoice invoices
					JOIN Invoice.tbType invoice_type ON invoices.InvoiceTypeCode = invoice_type.InvoiceTypeCode
					JOIN Invoice.tbTask tasks ON invoices.InvoiceNumber = tasks.InvoiceNumber
				WHERE invoices.InvoicedOn >= @StartOn
					AND tasks.CashCode = @CashCode
			), active_tasks AS
			(
				SELECT StartOn, SUM(InvoiceValue) InvoiceValue, SUM(InvoiceTax) InvoiceTax
				FROM active_candidates
				GROUP BY StartOn
			), active_items AS
			(
				SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod AS p WHERE (StartOn <= invoices.InvoicedOn) ORDER BY StartOn DESC) AS StartOn,
						CASE WHEN invoice_type.CashModeCode = 0 THEN items.InvoiceValue * - 1 ELSE items.InvoiceValue END AS InvoiceValue, 
						CASE WHEN invoice_type.CashModeCode = 0 THEN items.TaxValue * - 1 ELSE items.TaxValue END AS InvoiceTax
				FROM Invoice.tbInvoice invoices
					JOIN Invoice.tbType invoice_type ON invoices.InvoiceTypeCode = invoice_type.InvoiceTypeCode
					JOIN Invoice.tbItem items ON invoices.InvoiceNumber = items.InvoiceNumber
				WHERE invoices.InvoicedOn >= @StartOn AND items.CashCode = @CashCode
			), active_invoices AS
			(
				SELECT StartOn, InvoiceValue, InvoiceTax FROM active_tasks
				UNION
				SELECT StartOn, InvoiceValue, InvoiceTax FROM active_items
			), active_periods AS
			(
				SELECT StartOn, SUM(InvoiceValue) AS InvoiceValue, SUM(InvoiceTax) AS InvoiceTax
				FROM active_invoices
				GROUP BY StartOn
			)
			UPDATE cashcode_values
			SET InvoiceValue += ABS(active_periods.InvoiceValue), InvoiceTax += ABS(active_periods.InvoiceTax)
			FROM @tbReturn cashcode_values JOIN active_periods ON cashcode_values.StartOn = active_periods.StartOn;

			IF @IsTaxCode <> 0
				BEGIN
				IF EXISTS (SELECT CashCode FROM Cash.tbTaxType WHERE CashCode = @CashCode AND TaxTypeCode = 0)
					BEGIN	
					WITH ct_due AS
					(
						SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod AS p WHERE (StartOn <= ct_statement.StartOn) ORDER BY StartOn DESC) AS StartOn, TaxDue 
						FROM Cash.vwTaxCorpStatement ct_statement
						WHERE ct_statement.StartOn >= @StartOn
					)							
					UPDATE cashcode_values
					SET InvoiceValue += TaxDue
					FROM ct_due
						JOIN @tbReturn cashcode_values ON ct_due.StartOn = cashcode_values.StartOn;	
					END

				IF EXISTS (SELECT CashCode FROM Cash.tbTaxType WHERE CashCode = @CashCode AND TaxTypeCode = 1)
					BEGIN			
					WITH vat_due AS
					(
						SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod AS p WHERE (StartOn <= vat_statement.StartOn) ORDER BY StartOn DESC) AS StartOn, VatDue 
						FROM Cash.vwTaxVatStatement vat_statement
						WHERE vat_statement.StartOn >= @StartOn
					)
					UPDATE cashcode_values
					SET InvoiceValue += VatDue
					FROM vat_due
						JOIN @tbReturn cashcode_values ON vat_due.StartOn = cashcode_values.StartOn;		
					END
				END
			END

		IF (@IncludeOrderBook <> 0)
			BEGIN
			WITH tasks AS
			(
				SELECT task.TaskCode,
						(SELECT        TOP (1) StartOn
						FROM            App.tbYearPeriod
						WHERE        (StartOn <= task.ActionOn)
						ORDER BY StartOn DESC) AS StartOn, task.TotalCharge, ISNULL(tax.TaxRate, 0) AS TaxRate
				FROM            Task.tbTask AS task INNER JOIN
											App.tbTaxCode AS tax ON task.TaxCode = tax.TaxCode
				WHERE     (task.CashCode = @CashCode) AND ((task.TaskStatusCode = 1) OR (task.TaskStatusCode = 2))
			), tasks_foryear AS
			(
				SELECT tasks.TaskCode, tasks.StartOn, tasks.TotalCharge, tasks.TaxRate
				FROM tasks
					JOIN @tbReturn invoice_history ON tasks.StartOn = invoice_history.StartOn		
			)
			, order_invoice_value AS
			(
				SELECT   invoices.TaskCode, tasks_foryear.StartOn, SUM(invoices.InvoiceValue) AS InvoiceValue, SUM(invoices.TaxValue) AS InvoiceTax
				FROM  Invoice.tbTask invoices
					JOIN tasks_foryear ON invoices.TaskCode = tasks_foryear.TaskCode 
				GROUP BY invoices.TaskCode, StartOn
			), orders AS
			(
				SELECT tasks_foryear.StartOn, 
					tasks_foryear.TotalCharge - ISNULL(order_invoice_value.InvoiceValue, 0) AS InvoiceValue,
					(tasks_foryear.TotalCharge * tasks_foryear.TaxRate) - ISNULL(order_invoice_value.InvoiceTax, 0) AS InvoiceTax
				FROM tasks_foryear LEFT OUTER JOIN order_invoice_value ON tasks_foryear.TaskCode = order_invoice_value.TaskCode
			), order_summary AS
			(
				SELECT StartOn, SUM(InvoiceValue) As InvoiceValue, SUM(InvoiceTax) As InvoiceTax
				FROM orders
				GROUP BY StartOn
			)
			UPDATE cashcode_values
			SET InvoiceValue += order_summary.InvoiceValue, InvoiceTax += order_summary.InvoiceTax
			FROM @tbReturn cashcode_values JOIN order_summary ON cashcode_values.StartOn = order_summary.StartOn;

			END
	
		IF (@IncludeTaxAccruals <> 0) AND (@IsTaxCode <> 0)
			BEGIN
			IF EXISTS (SELECT CashCode FROM Cash.tbTaxType WHERE CashCode = @CashCode AND TaxTypeCode = 0)
				BEGIN
				WITH ct_dates AS
				(
					SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
				), ct_period AS
				(
					SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= PayOn) ORDER BY StartOn DESC) AS StartOn, PayOn, PayFrom, PayTo
					FROM  ct_dates 
						JOIN  App.tbYearPeriod AS year_period ON ct_dates.PayTo = year_period.StartOn 
						JOIN App.tbYear AS y ON year_period.YearNumber = y.YearNumber 
					WHERE     year_period.StartOn >= (SELECT StartOn FROM App.vwActivePeriod)
				), ct_accrual_details AS
				(		
					SELECT StartOn, SUM(TaxDue) AS TaxDue 
					FROM Cash.vwTaxCorpAccruals
					WHERE TaxDue <> 0
					GROUP BY StartOn
				), ct_accruals AS
				(
					SELECT (SELECT ct_period.StartOn FROM ct_period WHERE ct_accrual_details.StartOn >= ct_period.PayFrom AND ct_accrual_details.StartOn < ct_period.PayTo) AS StartOn, TaxDue
					FROM ct_accrual_details
				), ct_due AS
				(
					SELECT StartOn, SUM(TaxDue) AS TaxDue
					FROM ct_accruals
					GROUP BY StartOn
				)
				UPDATE cashcode_values
				SET InvoiceValue += TaxDue
				FROM ct_due
					JOIN @tbReturn cashcode_values ON ct_due.StartOn = cashcode_values.StartOn;	

				END

			IF EXISTS (SELECT CashCode FROM Cash.tbTaxType WHERE CashCode = @CashCode AND TaxTypeCode = 1)
				BEGIN
				WITH vat_dates AS
				(
					SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
				), vat_period AS
				(
					SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= PayOn) ORDER BY StartOn DESC) AS StartOn, PayOn, PayFrom, PayTo
					FROM  vat_dates 
						JOIN  App.tbYearPeriod AS year_period ON vat_dates.PayTo = year_period.StartOn 
						JOIN App.tbYear AS y ON year_period.YearNumber = y.YearNumber 
					WHERE     (y.CashStatusCode = 1) OR (y.CashStatusCode = 2)
				), vat_accrual_details AS
				(		
					SELECT StartOn, SUM(VatDue) AS VatDue 
					FROM Cash.vwTaxVatAccruals
					WHERE VatDue <> 0
					GROUP BY StartOn
				), vat_accruals AS
				(
					SELECT (SELECT vat_period.StartOn FROM vat_period WHERE vat_accrual_details.StartOn >= vat_period.PayFrom AND vat_accrual_details.StartOn < vat_period.PayTo) AS StartOn, VatDue
					FROM vat_accrual_details
				), vat_due AS
				(
					SELECT StartOn, SUM(VatDue) AS VatDue
					FROM vat_accruals
					GROUP BY StartOn
				)
				UPDATE cashcode_values
				SET InvoiceValue += VatDue
				FROM vat_due
					JOIN @tbReturn cashcode_values ON vat_due.StartOn = cashcode_values.StartOn;	
				END
			END

		SELECT StartOn, InvoiceValue, InvoiceTax, ForecastValue, ForecastTax FROM @tbReturn;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

