CREATE VIEW Invoice.vwStatusLive
AS
	WITH nonzero_balance_orgs AS
	(
		SELECT AccountCode, ABS(Balance) Balance, CASE WHEN Balance > 0 THEN 0 ELSE 1 END CashModeCode 
		FROM Org.vwCurrentBalance
	)
	, paid_invoices AS
	(
		SELECT AccountCode, InvoiceNumber, 3 InvoiceStatusCode, TotalPaid, TaxRate
		FROM nonzero_balance_orgs
			CROSS APPLY
				(
					SELECT InvoiceNumber,
						(InvoiceValue + TaxValue) TotalPaid,
						TaxValue / CASE InvoiceValue WHEN 0 THEN 1 ELSE InvoiceValue END TaxRate
					FROM Invoice.tbInvoice invoices
						INNER JOIN Invoice.tbType ON invoices.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
					WHERE (AccountCode = nonzero_balance_orgs.AccountCode 
							AND Invoice.tbType.CashModeCode <> nonzero_balance_orgs.CashModeCode)
				) invoices
	), candidates_invoices AS
	(
		SELECT AccountCode, NULL InvoiceNumber, 0 RowNumber, Balance TotalCharge, 0 TaxRate
		FROM nonzero_balance_orgs
		UNION
		SELECT AccountCode, InvoiceNumber, RowNumber, TotalCharge, TaxRate
		FROM nonzero_balance_orgs
			CROSS APPLY
				(
					SELECT InvoiceNumber, ROW_NUMBER() OVER (ORDER BY InvoicedOn DESC) RowNumber,
							(InvoiceValue + TaxValue) * - 1  TotalCharge,
							TaxValue / CASE InvoiceValue WHEN 0 THEN 1 ELSE InvoiceValue END TaxRate
					FROM Invoice.tbInvoice invoices
						INNER JOIN Invoice.tbType ON invoices.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
					WHERE AccountCode = nonzero_balance_orgs.AccountCode 
						AND Invoice.tbType.CashModeCode = nonzero_balance_orgs.CashModeCode
				) invoices
	)
	, candidate_balance AS
	(
		SELECT AccountCode, InvoiceNumber, TotalCharge, TaxRate,
			CAST(SUM(TotalCharge) OVER (PARTITION BY AccountCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS float) AS Balance
		FROM candidates_invoices
	), candidate_status AS
	(
		SELECT AccountCode, InvoiceNumber,
			CASE 
				WHEN Balance >= 0 THEN 1 ELSE
				CASE WHEN TotalCharge < Balance THEN 2 ELSE 3 END
			END InvoiceStatusCode,
			CASE 
				WHEN Balance >= 0 THEN 0 ELSE
				CASE WHEN TotalCharge < Balance THEN ABS(Balance) ELSE ABS(TotalCharge) END
			END TotalPaid,
			TaxRate
		FROM candidate_balance
	), invoice_status AS
	(
		SELECT AccountCode, InvoiceNumber, InvoiceStatusCode, TotalPaid, TaxRate 
		FROM paid_invoices
		UNION
		SELECT AccountCode, InvoiceNumber, InvoiceStatusCode, TotalPaid, TaxRate 
		FROM candidate_status 
		WHERE NOT (InvoiceNumber IS NULL)
	)
	SELECT AccountCode, InvoiceNumber, InvoiceStatusCode, 
		TotalPaid / (1 + TaxRate) PaidValue,
		TotalPaid - (TotalPaid / (1 + TaxRate)) PaidTaxValue
	FROM invoice_status;
