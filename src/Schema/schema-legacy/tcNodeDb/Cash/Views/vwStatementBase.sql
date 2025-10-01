CREATE   VIEW Cash.vwStatementBase
AS
	--invoiced taxes
	WITH corp_taxcode AS
	(
		SELECT TOP (1) AccountCode, CashCode 
		FROM Cash.tbTaxType WHERE (TaxTypeCode = 0)
	), corptax_invoiced_entries AS
	(
		SELECT AccountCode, CashCode, StartOn, TaxDue, Balance,
			ROW_NUMBER() OVER (ORDER BY StartOn) AS RowNumber 
		FROM Cash.vwTaxCorpStatement CROSS JOIN corp_taxcode
		WHERE (Balance <> 0) AND (StartOn >= (SELECT MIN(StartOn) FROM App.tbYearPeriod WHERE CashStatusCode < 2)) --AND (TaxDue > 0) 
	), corptax_invoiced_owing AS
	(
		SELECT AccountCode, CashCode EntryDescription, StartOn AS TransactOn, 4 AS CashEntryTypeCode, 
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1214) ReferenceCode, 0 AS PayIn,
			CASE RowNumber WHEN 1 THEN Balance ELSE TaxDue END AS PayOut
		FROM corptax_invoiced_entries
	), vat_taxcode AS
	(
		SELECT TOP (1) AccountCode, CashCode 
		FROM Cash.tbTaxType WHERE (TaxTypeCode = 1)
	), vat_totals AS
	(
		SELECT ROW_NUMBER() OVER (ORDER BY RowNumber DESC) AS Id, StartOn AS TransactOn, VatDue,
			CASE WHEN VatPaid  < 0 OR Balance <= 0 THEN NULL ELSE 1 END IsLive
		FROM Cash.vwTaxVatStatement
		--WHERE VatDue <> 0
	), vat_invoiced_owing AS
	(
		SELECT AccountCode, CashCode EntryDescription, TransactOn, 5 AS CashEntryTypeCode, 
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1214) ReferenceCode, 
			CASE WHEN VatDue < 0 THEN ABS(VatDue) ELSE 0 END AS PayIn,
			CASE WHEN VatDue >= 0 THEN VatDue ELSE 0 END AS PayOut
		FROM vat_totals CROSS JOIN vat_taxcode
		WHERE Id <  COALESCE((SELECT TOP 1 t.Id FROM vat_totals t WHERE t.IsLive IS NULL ORDER BY Id), (SELECT MIN(Id) + 1 FROM vat_totals))
		--(SELECT TOP 1 t.Id FROM vat_totals t WHERE t.IsLive IS NULL ORDER BY Id)
	)
	--uninvoiced taxes
	,  corptax_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(0)
	), corptax_accrual_entries AS
	(
		SELECT StartOn, SUM(TaxDue) AS TaxDue
		FROM Cash.vwTaxCorpAccruals
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
	)	
	, corptax_accruals AS
	(	
		SELECT AccountCode, CashCode EntryDescription, TransactOn, 4 AS CashEntryTypeCode, 
				(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1215) ReferenceCode, 
				CASE WHEN TaxDue < 0 THEN ABS(TaxDue) ELSE 0 END AS PayIn,
				CASE WHEN TaxDue >= 0 THEN TaxDue ELSE 0 END AS PayOut
		FROM corptax_accrual_totals CROSS JOIN corp_taxcode
	), vat_dates AS
	(
		SELECT PayOn, PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
	), vat_accrual_entries AS
	(
		SELECT StartOn, SUM(VatDue) AS TaxDue 
		FROM Cash.vwTaxVatAccruals vat_audit
		WHERE vat_audit.VatDue <> 0
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
	), vat_accruals AS
	(
		SELECT vat_taxcode.AccountCode, vat_taxcode.CashCode EntryDescription, TransactOn, 5 AS CashEntryTypeCode, 
				(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 1215) ReferenceCode,
				CASE WHEN TaxDue < 0 THEN ABS(TaxDue) ELSE 0 END AS PayIn,
				CASE WHEN TaxDue >= 0 THEN TaxDue ELSE 0 END AS PayOut
		FROM vat_accrual_totals
			CROSS JOIN vat_taxcode
	)
	--unpaid invoices
	, invoice_desc_candidates AS
	(
		SELECT invoice_tasks.InvoiceNumber, 0 OrderBy, 
			FIRST_VALUE(invoiced_task.ActivityCode) OVER (PARTITION BY invoice_tasks.InvoiceNumber ORDER BY invoice_tasks.TaskCode) EntryDescription
		FROM Invoice.tbTask invoice_tasks 
			JOIN Task.tbTask invoiced_task ON invoice_tasks.TaskCode = invoiced_task.TaskCode
			JOIN Invoice.tbInvoice invoices ON invoices.InvoiceNumber = invoice_tasks.InvoiceNumber
		WHERE  (InvoiceStatusCode BETWEEN 1 AND 2)
		UNION
		SELECT invoice_items.InvoiceNumber, 1 OrderBy, 
			FIRST_VALUE(cash_code.CashDescription) OVER (PARTITION BY invoice_items.InvoiceNumber ORDER BY invoice_items.CashCode) EntryDescription
		FROM Invoice.tbItem invoice_items 
			JOIN Cash.tbCode cash_code ON invoice_items.CashCode = cash_code.CashCode
			JOIN Invoice.tbInvoice invoices ON invoices.InvoiceNumber = invoice_items.InvoiceNumber
		WHERE  (InvoiceStatusCode BETWEEN 1 AND 2)
	), invoice_desc AS
	(
		SELECT InvoiceNumber,
			FIRST_VALUE(EntryDescription) OVER (PARTITION BY InvoiceNumber ORDER BY OrderBy) EntryDescription
		FROM invoice_desc_candidates
	), invoices_outstanding AS
	(
		SELECT  invoices.AccountCode, invoice_desc.EntryDescription, invoices.ExpectedOn AS TransactOn, 1 AS CashEntryTypeCode, invoices.InvoiceNumber AS ReferenceCode, 
					CASE CashModeCode WHEN 1 THEN InvoiceValue + TaxValue - (PaidValue + PaidTaxValue) ELSE 0 END AS PayIn, 
					CASE CashModeCode WHEN 0 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) ELSE 0 END AS PayOut
		FROM  Invoice.tbInvoice invoices
			JOIN Invoice.tbType invoice_type ON invoices.InvoiceTypeCode = invoice_type.InvoiceTypeCode
			JOIN invoice_desc ON invoices.InvoiceNumber = invoice_desc.InvoiceNumber
		WHERE  (InvoiceStatusCode < 3) AND ((InvoiceValue + TaxValue - PaidValue + PaidTaxValue) > 0)
	), task_invoiced_quantity AS
	(
		SELECT        Invoice.tbTask.TaskCode, SUM(Invoice.tbTask.Quantity) AS InvoiceQuantity
		FROM            Invoice.tbTask INNER JOIN
								 Invoice.tbInvoice ON Invoice.tbTask.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0) OR
								 (Invoice.tbInvoice.InvoiceTypeCode = 2)
		GROUP BY Invoice.tbTask.TaskCode
	), tasks_confirmed AS
	(
		SELECT Task.tbTask.TaskCode AS ReferenceCode, Task.tbTask.AccountCode, Task.tbTask.PaymentOn AS TransactOn, Task.tbTask.PaymentOn, 2 AS CashEntryTypeCode, 
								 CASE WHEN Cash.tbCategory.CashModeCode = 0 THEN (Task.tbTask.UnitCharge + Task.tbTask.UnitCharge * App.tbTaxCode.TaxRate) * (Task.tbTask.Quantity - ISNULL(task_invoiced_quantity.InvoiceQuantity, 
								 0)) ELSE 0 END AS PayOut, CASE WHEN Cash.tbCategory.CashModeCode = 1 THEN (Task.tbTask.UnitCharge + Task.tbTask.UnitCharge * App.tbTaxCode.TaxRate) 
								 * (Task.tbTask.Quantity - ISNULL(task_invoiced_quantity.InvoiceQuantity, 0)) ELSE 0 END AS PayIn, Task.tbTask.ActivityCode EntryDescription
		FROM            App.tbTaxCode INNER JOIN
								 Task.tbTask ON App.tbTaxCode.TaxCode = Task.tbTask.TaxCode INNER JOIN
								 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode LEFT OUTER JOIN
								 task_invoiced_quantity ON Task.tbTask.TaskCode = task_invoiced_quantity.TaskCode
		WHERE        (Task.tbTask.TaskStatusCode > 0) AND (Task.tbTask.TaskStatusCode < 3) AND (Task.tbTask.Quantity - ISNULL(task_invoiced_quantity.InvoiceQuantity, 0) > 0)
	)
	--interbank transfers
	, transfer_current_account AS
	(
		SELECT        Org.tbAccount.CashAccountCode
		FROM            Org.tbAccount INNER JOIN
								 Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode INNER JOIN
								 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE        (Cash.tbCategory.CashTypeCode = 2)
	), transfer_accruals AS
	(
		SELECT        Cash.tbPayment.AccountCode, Cash.tbPayment.CashCode EntryDescription, Cash.tbPayment.PaidOn AS TransactOn, Cash.tbPayment.PaymentCode AS ReferenceCode, 
			6 AS CashEntryTypeCode, Cash.tbPayment.PaidInValue AS PayIn, Cash.tbPayment.PaidOutValue AS PayOut
		FROM            transfer_current_account INNER JOIN
								 Cash.tbPayment ON transfer_current_account.CashAccountCode = Cash.tbPayment.CashAccountCode
		WHERE        (Cash.tbPayment.PaymentStatusCode = 2)
	)
	SELECT AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM corptax_invoiced_owing
	UNION
	SELECT AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM vat_invoiced_owing
	UNION
	SELECT AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM corptax_accruals
	UNION
	SELECT AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM vat_accruals
	UNION
	SELECT AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM invoices_outstanding
	UNION 
	SELECT AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM tasks_confirmed
	UNION
	SELECT AccountCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut FROM transfer_accruals;
