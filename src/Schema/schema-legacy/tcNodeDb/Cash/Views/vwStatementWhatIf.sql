CREATE   VIEW Cash.vwStatementWhatIf
AS
	WITH quotes AS
	(
		SELECT Task.tbTask.TaskCode AS ReferenceCode, 
			Task.tbTask.AccountCode, Task.tbTask.PaymentOn AS TransactOn, 
			Task.tbTask.PaymentOn, 3 AS CashEntryTypeCode, 
			CASE WHEN Cash.tbCategory.CashModeCode = 0 
				THEN (Task.tbTask.UnitCharge + Task.tbTask.UnitCharge * App.tbTaxCode.TaxRate) * Task.tbTask.Quantity 
				ELSE 0 
			END AS PayOut, 
			CASE WHEN Cash.tbCategory.CashModeCode = 1 
				THEN (Task.tbTask.UnitCharge + Task.tbTask.UnitCharge * App.tbTaxCode.TaxRate) * Task.tbTask.Quantity ELSE 0 
			END AS PayIn, 
			Task.tbTask.ActivityCode EntryDescription
		FROM Task.vwCostSetTasks quoted_tasks 
			JOIN  Task.tbTask ON quoted_tasks.TaskCode = Task.tbTask.TaskCode 		
			JOIN App.tbTaxCode ON App.tbTaxCode.TaxCode = Task.tbTask.TaxCode 
			JOIN Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode 
			JOIN Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
	), cost_set_task_vat AS
	(
		SELECT  (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= quotes.ActionOn) ORDER BY p.StartOn DESC) AS StartOn,  
				quotes.TaskCode, quotes.TaxCode,
				quotes.Quantity AS QuantityRemaining,
				quotes.UnitCharge * quotes.Quantity AS TotalValue, 
				quotes.UnitCharge * quotes.Quantity * App.tbTaxCode.TaxRate AS TaxValue,
				App.tbTaxCode.TaxRate,
				Cash.tbCategory.CashModeCode
		FROM    Task.vwCostSetTasks cost_set INNER JOIN	Task.tbTask quotes ON cost_set.TaskCode = quotes.TaskCode INNER JOIN
				Org.tbOrg ON quotes.AccountCode = Org.tbOrg.AccountCode INNER JOIN
				Cash.tbCode ON quotes.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
				App.tbTaxCode ON quotes.TaxCode = App.tbTaxCode.TaxCode 
		WHERE        (App.tbTaxCode.TaxTypeCode = 1) AND (App.tbTaxCode.TaxTypeCode = 1)
			AND (quotes.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) FROM App.tbOptions))
	), cost_set_vat_accruals AS
	(
		SELECT StartOn, TaskCode, TaxCode, QuantityRemaining, TotalValue, TaxValue, TaxRate,
			CASE CashModeCode WHEN 0 THEN TaxValue * -1 ELSE TaxValue END VatDue
		FROM cost_set_task_vat
	), vat_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
	), vat_accrual_entries AS
	(
		SELECT StartOn, SUM(VatDue) AS TaxDue 
		FROM cost_set_vat_accruals
		WHERE VatDue <> 0
		GROUP BY StartOn
	), vat_accrual_candidates AS
	(
		SELECT (SELECT PayOn FROM vat_dates WHERE vat_accrual_entries.StartOn >= PayFrom AND vat_accrual_entries.StartOn < PayTo) AS TransactOn, TaxDue			
		FROM vat_accrual_entries 
	), vat_accrual_totals AS
	(
		SELECT TransactOn, SUM(TaxDue) AS TaxDue
		FROM vat_accrual_candidates
		GROUP BY TransactOn
	), vat_taxcode AS
	(
		SELECT TOP (1) AccountCode, CashCode 
		FROM Cash.tbTaxType WHERE (TaxTypeCode = 1)
	), vat_accruals AS
	(
		SELECT vat_taxcode.AccountCode, vat_taxcode.CashCode EntryDescription, TransactOn, 5 AS CashEntryTypeCode, 
				(SELECT CashEntryType FROM Cash.tbEntryType WHERE CashEntryTypeCode = 3) ReferenceCode,
				CASE WHEN TaxDue < 0 THEN ABS(TaxDue) ELSE 0 END AS PayIn,
				CASE WHEN TaxDue >= 0 THEN TaxDue ELSE 0 END AS PayOut
		FROM vat_accrual_totals
			CROSS JOIN vat_taxcode
	), cost_set_task_tax AS
	(
		SELECT (SELECT TOP (1) StartOn FROM App.tbYearPeriod WHERE (StartOn <= ActionOn) ORDER BY StartOn DESC) AS StartOn, 
			CASE WHEN Cash.tbCategory.CashModeCode = 0 THEN quote.TotalCharge * - 1 ELSE quote.TotalCharge END AS TotalCharge
		FROM Task.vwCostSetTasks cost_set INNER JOIN
			Task.tbTask AS quote ON cost_set.TaskCode = quote.TaskCode INNER JOIN
								 Cash.tbCode ON quote.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE    (quote.ActionOn <= (SELECT DATEADD(d, TaxHorizon, CURRENT_TIMESTAMP) AS HorizonOn FROM App.tbOptions))
	), cost_set_corptax AS
	(
		SELECT cost_set_task_tax.StartOn, TotalCharge, TotalCharge * CorporationTaxRate AS TaxDue
		FROM cost_set_task_tax JOIN App.tbYearPeriod year_period ON cost_set_task_tax.StartOn = year_period.StartOn
	), corptax_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), corptax_accrual_entries AS
	(
		SELECT StartOn, SUM(TaxDue) AS TaxDue
		FROM cost_set_corptax
		GROUP BY StartOn
	), corptax_accrual_candidates AS
	(
			SELECT (SELECT PayOn FROM corptax_dates WHERE corptax_accrual_entries.StartOn >= PayFrom AND corptax_accrual_entries.StartOn < PayTo) AS TransactOn, TaxDue			
		FROM corptax_accrual_entries 
	), corptax_accrual_totals AS
	(
		SELECT TransactOn, SUM(TaxDue) AS TaxDue
		FROM corptax_accrual_candidates
		GROUP BY TransactOn
	), corp_taxcode AS
	(
		SELECT TOP (1) AccountCode, CashCode 
		FROM Cash.tbTaxType WHERE (TaxTypeCode = 0)
	), corptax_accruals AS
	(	
		SELECT AccountCode, CashCode EntryDescription, TransactOn, 4 AS CashEntryTypeCode, 
				(SELECT CashEntryType FROM Cash.tbEntryType WHERE CashEntryTypeCode = 3) ReferenceCode, 
				CASE WHEN TaxDue < 0 THEN ABS(TaxDue) ELSE 0 END AS PayIn,
				CASE WHEN TaxDue >= 0 THEN TaxDue ELSE 0 END AS PayOut
		FROM corptax_accrual_totals CROSS JOIN corp_taxcode
	), cost_statement AS
	(
		SELECT AccountCode, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut, EntryDescription FROM Cash.vwStatementBase
		UNION
		SELECT AccountCode, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut, EntryDescription FROM quotes
		UNION
		SELECT AccountCode, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut, EntryDescription FROM vat_accruals
		UNION
		SELECT AccountCode, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut, EntryDescription FROM corptax_accruals
	), statement_base AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY TransactOn, CashEntryTypeCode DESC) AS RowNumber,
		 AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut 
		 FROM cost_statement
	), opening_balance AS
	(	
		SELECT SUM( Org.tbAccount.CurrentBalance) AS OpeningBalance
		FROM         Org.tbAccount INNER JOIN
							  Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode
		WHERE     ( Org.tbAccount.AccountClosed = 0) AND (Org.tbAccount.AccountTypeCode = 0)
	), statement_data AS
	(
		SELECT 
			0 AS RowNumber,
			(SELECT TOP (1) AccountCode FROM App.tbOptions) AS AccountCode,
			NULL AS EntryDescription,
			NULL AS TransactOn,    
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 3013) AS ReferenceCode,	
			1 AS CashEntryTypeCode,
			PayIn = (SELECT OpeningBalance FROM opening_balance),
			0 AS PayOut
		UNION 
		SELECT RowNumber, AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut 
		FROM statement_base
	), company_statement AS
	(
		SELECT RowNumber, AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut,
			SUM(PayIn + (PayOut * -1)) OVER (ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM statement_data
	)
	SELECT RowNumber, cs.AccountCode, org.AccountName, cs.EntryDescription,
			TransactOn, ReferenceCode, cs.CashEntryTypeCode, et.CashEntryType, CAST(PayIn AS decimal(18, 5)) PayIn, CAST(PayOut AS decimal(18, 5)) PayOut, CAST(Balance AS decimal(18, 5)) Balance
	FROM company_statement cs 
		JOIN Org.tbOrg org ON cs.AccountCode = org.AccountCode
		JOIN Cash.tbEntryType et ON cs.CashEntryTypeCode = et.CashEntryTypeCode;
