CREATE VIEW Subject.vwBalanceOutstanding
AS
	WITH invoices_unpaid AS
	(
		SELECT        Invoice.tbInvoice.SubjectCode, 
			CASE Invoice.tbType.CashPolarityCode 
				WHEN 0 THEN ((InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue)) * - 1 
				WHEN 1 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) END AS OutstandingValue
		FROM            Invoice.tbInvoice INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
		WHERE        (Invoice.tbInvoice.InvoiceStatusCode > 0) AND (Invoice.tbInvoice.InvoiceStatusCode < 3) 
	), current_balance AS
	(
		SELECT SubjectCode, SUM(OutstandingValue) AS Balance
		FROM   invoices_unpaid	
		GROUP BY SubjectCode
	)
	SELECT Subject.SubjectCode, ISNULL(current_balance.Balance, 0) AS Balance
	FROM Subject.tbSubject Subject 
		LEFT OUTER JOIN current_balance ON Subject.SubjectCode = current_balance.SubjectCode;
