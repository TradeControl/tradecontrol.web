CREATE VIEW Cash.vwTaxVatAuditInvoices
AS
	WITH vat_transactions AS
	(
		SELECT
			i.InvoicedOn,
			i.InvoiceNumber,
			i.InvoiceTypeCode,
			it.TaxCode,
			it.InvoiceValue,
			it.TaxValue,
			ROUND(it.TaxValue / it.InvoiceValue, 3) AS CalcRate,
			tc.TaxRate,
			ISNULL(v.EUJurisdiction, 0) AS EUJurisdiction,
			it.CashCode AS IdentityCode,
			c.CashDescription AS ItemDescription
		FROM Invoice.tbItem AS it
			INNER JOIN Invoice.tbInvoice AS i
				ON it.InvoiceNumber = i.InvoiceNumber
			INNER JOIN Subject.tbSubject AS s
				ON i.SubjectCode = s.SubjectCode
			LEFT OUTER JOIN Subject.tbVirtual AS v
				ON s.SubjectCode = v.SubjectCode
			INNER JOIN App.tbTaxCode AS tc
				ON it.TaxCode = tc.TaxCode
			INNER JOIN Cash.tbCode AS c
				ON it.CashCode = c.CashCode
		WHERE tc.TaxTypeCode = 1
			AND it.InvoiceValue <> 0

		UNION

		SELECT
			i.InvoicedOn,
			ip.InvoiceNumber,
			i.InvoiceTypeCode,
			ip.TaxCode,
			ip.InvoiceValue,
			ip.TaxValue,
			ROUND(ip.TaxValue / ip.InvoiceValue, 3) AS CalcRate,
			tc.TaxRate,
			ISNULL(v.EUJurisdiction, 0) AS EUJurisdiction,
			ip.ProjectCode AS IdentityCode,
			p.ProjectTitle AS ItemDescription
		FROM Invoice.tbProject AS ip
			INNER JOIN Invoice.tbInvoice AS i
				ON ip.InvoiceNumber = i.InvoiceNumber
			INNER JOIN Subject.tbSubject AS s
				ON i.SubjectCode = s.SubjectCode
			LEFT OUTER JOIN Subject.tbVirtual AS v
				ON s.SubjectCode = v.SubjectCode
			INNER JOIN App.tbTaxCode AS tc
				ON ip.TaxCode = tc.TaxCode
			INNER JOIN Project.tbProject AS p
				ON ip.ProjectCode = p.ProjectCode
		WHERE tc.TaxTypeCode = 1
			AND ip.InvoiceValue <> 0
	),
	vat_dataset AS
	(
		SELECT
			(
				SELECT due_dates.PayTo
				FROM Cash.fnTaxTypeDueDates(1, 0) AS due_dates
				WHERE vt.InvoicedOn >= due_dates.PayFrom
					AND vt.InvoicedOn < due_dates.PayTo
			) AS StartOn,
			vt.InvoicedOn,
			vt.InvoiceNumber,
			it.InvoiceType,
			vt.InvoiceTypeCode,
			vt.TaxCode,
			vt.InvoiceValue,
			vt.TaxValue,
			vt.TaxRate,
			vt.EUJurisdiction,
			vt.IdentityCode,
			vt.ItemDescription,
			CASE
				WHEN vt.EUJurisdiction = 0 THEN
					CASE vt.InvoiceTypeCode
						WHEN 0 THEN vt.InvoiceValue
						WHEN 1 THEN vt.InvoiceValue * -1
						ELSE 0
					END
				ELSE 0
			END AS HomeSales,
			CASE
				WHEN vt.EUJurisdiction = 0 THEN
					CASE vt.InvoiceTypeCode
						WHEN 2 THEN vt.InvoiceValue
						WHEN 3 THEN vt.InvoiceValue * -1
						ELSE 0
					END
				ELSE 0
			END AS HomePurchases,
			CASE
				WHEN vt.EUJurisdiction <> 0 THEN
					CASE vt.InvoiceTypeCode
						WHEN 0 THEN vt.InvoiceValue
						WHEN 1 THEN vt.InvoiceValue * -1
						ELSE 0
					END
				ELSE 0
			END AS ExportSales,
			CASE
				WHEN vt.EUJurisdiction <> 0 THEN
					CASE vt.InvoiceTypeCode
						WHEN 2 THEN vt.InvoiceValue
						WHEN 3 THEN vt.InvoiceValue * -1
						ELSE 0
					END
				ELSE 0
			END AS ExportPurchases,
			CASE
				WHEN vt.EUJurisdiction = 0 THEN
					CASE vt.InvoiceTypeCode
						WHEN 0 THEN vt.TaxValue
						WHEN 1 THEN vt.TaxValue * -1
						ELSE 0
					END
				ELSE 0
			END AS HomeSalesVat,
			CASE
				WHEN vt.EUJurisdiction = 0 THEN
					CASE vt.InvoiceTypeCode
						WHEN 2 THEN vt.TaxValue
						WHEN 3 THEN vt.TaxValue * -1
						ELSE 0
					END
				ELSE 0
			END AS HomePurchasesVat,
			CASE
				WHEN vt.EUJurisdiction <> 0 THEN
					CASE vt.InvoiceTypeCode
						WHEN 0 THEN vt.TaxValue
						WHEN 1 THEN vt.TaxValue * -1
						ELSE 0
					END
				ELSE 0
			END AS ExportSalesVat,
			CASE
				WHEN vt.EUJurisdiction <> 0 THEN
					CASE vt.InvoiceTypeCode
						WHEN 2 THEN vt.TaxValue
						WHEN 3 THEN vt.TaxValue * -1
						ELSE 0
					END
				ELSE 0
			END AS ExportPurchasesVat
		FROM vat_transactions AS vt
			INNER JOIN Invoice.tbType AS it
				ON vt.InvoiceTypeCode = it.InvoiceTypeCode
	)
	SELECT
		CONCAT(y.Description, ' ', m.MonthName) AS YearPeriod,
		vd.*,
		(HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS VatDue
	FROM vat_dataset AS vd
		INNER JOIN App.tbYearPeriod AS yp
			ON vd.StartOn = yp.StartOn
		INNER JOIN App.tbYear AS y
			ON yp.YearNumber = y.YearNumber
		INNER JOIN App.tbMonth AS m
			ON yp.MonthNumber = m.MonthNumber;
GO
