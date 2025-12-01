CREATE VIEW Invoice.vwStatusLive
AS
	WITH nonzero_balance_Subjects AS
	(
		SELECT SubjectCode, ABS(Balance) Balance, CASE WHEN Balance > 0 THEN 0 ELSE 1 END CashPolarityCode 
		FROM Subject.vwCurrentBalance
	)
	, paid_invoices AS
	(
		SELECT SubjectCode, InvoiceNumber, 3 InvoiceStatusCode, TotalPaid, TaxRate
		FROM nonzero_balance_Subjects
			CROSS APPLY
				(
					SELECT InvoiceNumber,
						(InvoiceValue + TaxValue) TotalPaid,
						TaxValue / CASE InvoiceValue WHEN 0 THEN 1 ELSE InvoiceValue END TaxRate
					FROM Invoice.tbInvoice invoices
						INNER JOIN Invoice.tbType ON invoices.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
					WHERE (SubjectCode = nonzero_balance_Subjects.SubjectCode 
							AND Invoice.tbType.CashPolarityCode <> nonzero_balance_Subjects.CashPolarityCode)
				) invoices
	), candidates_invoices AS
	(
		SELECT SubjectCode, NULL InvoiceNumber, 0 RowNumber, Balance TotalCharge, 0 TaxRate
		FROM nonzero_balance_Subjects
		UNION
		SELECT SubjectCode, InvoiceNumber, RowNumber, TotalCharge, TaxRate
		FROM nonzero_balance_Subjects
			CROSS APPLY
				(
					SELECT InvoiceNumber, ROW_NUMBER() OVER (ORDER BY InvoicedOn DESC) RowNumber,
							(InvoiceValue + TaxValue) * - 1  TotalCharge,
							TaxValue / CASE InvoiceValue WHEN 0 THEN 1 ELSE InvoiceValue END TaxRate
					FROM Invoice.tbInvoice invoices
						INNER JOIN Invoice.tbType ON invoices.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
					WHERE SubjectCode = nonzero_balance_Subjects.SubjectCode 
						AND Invoice.tbType.CashPolarityCode = nonzero_balance_Subjects.CashPolarityCode
				) invoices
	)
	, candidate_balance AS
	(
		SELECT SubjectCode, InvoiceNumber, TotalCharge, TaxRate,
			CAST(SUM(TotalCharge) OVER (PARTITION BY SubjectCode ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS float) AS Balance
		FROM candidates_invoices
	), candidate_status AS
	(
		SELECT SubjectCode, InvoiceNumber,
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
		SELECT SubjectCode, InvoiceNumber, InvoiceStatusCode, TotalPaid, TaxRate 
		FROM paid_invoices
		UNION
		SELECT SubjectCode, InvoiceNumber, InvoiceStatusCode, TotalPaid, TaxRate 
		FROM candidate_status 
		WHERE NOT (InvoiceNumber IS NULL)
	)
	SELECT SubjectCode, InvoiceNumber, InvoiceStatusCode, 
		TotalPaid / (1 + TaxRate) PaidValue,
		TotalPaid - (TotalPaid / (1 + TaxRate)) PaidTaxValue
	FROM invoice_status;
