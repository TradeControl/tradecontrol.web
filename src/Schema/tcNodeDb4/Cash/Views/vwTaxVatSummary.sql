CREATE VIEW Cash.vwTaxVatSummary
AS
	WITH vat_transactions AS
	(
		SELECT
			(
				SELECT TOP (1) yp.StartOn
				FROM App.tbYearPeriod AS yp
				WHERE yp.StartOn <= i.InvoicedOn
				ORDER BY yp.StartOn DESC
			) AS StartOn,
			i.InvoiceNumber,
			i.InvoiceTypeCode,
			it.TaxCode,
			it.InvoiceValue,
			it.TaxValue,
			ISNULL(v.EUJurisdiction, 0) AS EUJurisdiction,
			it.CashCode AS IdentityCode
		FROM App.vwTaxVatCashCodes AS c
			INNER JOIN Invoice.tbItem AS it
				ON c.CashCode = it.CashCode
			INNER JOIN Invoice.tbInvoice AS i
				ON it.InvoiceNumber = i.InvoiceNumber
			INNER JOIN Subject.tbSubject AS s
				ON i.SubjectCode = s.SubjectCode
			LEFT OUTER JOIN Subject.tbVirtual AS v
				ON s.SubjectCode = v.SubjectCode
			INNER JOIN App.tbTaxCode AS tc
				ON it.TaxCode = tc.TaxCode
		WHERE tc.TaxTypeCode = 1

		UNION

		SELECT
			(
				SELECT TOP (1) yp.StartOn
				FROM App.tbYearPeriod AS yp
				WHERE yp.StartOn <= i.InvoicedOn
				ORDER BY yp.StartOn DESC
			) AS StartOn,
			ip.InvoiceNumber,
			i.InvoiceTypeCode,
			ip.TaxCode,
			ip.InvoiceValue,
			ip.TaxValue,
			ISNULL(v.EUJurisdiction, 0) AS EUJurisdiction,
			ip.ProjectCode AS IdentityCode
		FROM App.vwTaxVatCashCodes AS c
			INNER JOIN Invoice.tbProject AS ip
				ON c.CashCode = ip.CashCode
			INNER JOIN Invoice.tbInvoice AS i
				ON ip.InvoiceNumber = i.InvoiceNumber
			INNER JOIN Subject.tbSubject AS s
				ON i.SubjectCode = s.SubjectCode
			LEFT OUTER JOIN Subject.tbVirtual AS v
				ON s.SubjectCode = v.SubjectCode
			INNER JOIN App.tbTaxCode AS tc
				ON ip.TaxCode = tc.TaxCode
		WHERE tc.TaxTypeCode = 1
	),
	vat_detail AS
	(
		SELECT
			StartOn,
			TaxCode,
			CASE
				WHEN EUJurisdiction = 0 THEN
					CASE InvoiceTypeCode
						WHEN 0 THEN InvoiceValue
						WHEN 1 THEN InvoiceValue * -1
						ELSE 0
					END
				ELSE 0
			END AS HomeSales,
			CASE
				WHEN EUJurisdiction = 0 THEN
					CASE InvoiceTypeCode
						WHEN 2 THEN InvoiceValue
						WHEN 3 THEN InvoiceValue * -1
						ELSE 0
					END
				ELSE 0
			END AS HomePurchases,
			CASE
				WHEN EUJurisdiction <> 0 THEN
					CASE InvoiceTypeCode
						WHEN 0 THEN InvoiceValue
						WHEN 1 THEN InvoiceValue * -1
						ELSE 0
					END
				ELSE 0
			END AS ExportSales,
			CASE
				WHEN EUJurisdiction <> 0 THEN
					CASE InvoiceTypeCode
						WHEN 2 THEN InvoiceValue
						WHEN 3 THEN InvoiceValue * -1
						ELSE 0
					END
				ELSE 0
			END AS ExportPurchases,
			CASE
				WHEN EUJurisdiction = 0 THEN
					CASE InvoiceTypeCode
						WHEN 0 THEN TaxValue
						WHEN 1 THEN TaxValue * -1
						ELSE 0
					END
				ELSE 0
			END AS HomeSalesVat,
			CASE
				WHEN EUJurisdiction = 0 THEN
					CASE InvoiceTypeCode
						WHEN 2 THEN TaxValue
						WHEN 3 THEN TaxValue * -1
						ELSE 0
					END
				ELSE 0
			END AS HomePurchasesVat,
			CASE
				WHEN EUJurisdiction <> 0 THEN
					CASE InvoiceTypeCode
						WHEN 0 THEN TaxValue
						WHEN 1 THEN TaxValue * -1
						ELSE 0
					END
				ELSE 0
			END AS ExportSalesVat,
			CASE
				WHEN EUJurisdiction <> 0 THEN
					CASE InvoiceTypeCode
						WHEN 2 THEN TaxValue
						WHEN 3 THEN TaxValue * -1
						ELSE 0
					END
				ELSE 0
			END AS ExportPurchasesVat
		FROM vat_transactions
	),
	vatcode_summary AS
	(
		SELECT
			StartOn,
			TaxCode,
			SUM(HomeSales) AS HomeSales,
			SUM(HomePurchases) AS HomePurchases,
			SUM(ExportSales) AS ExportSales,
			SUM(ExportPurchases) AS ExportPurchases,
			SUM(HomeSalesVat) AS HomeSalesVat,
			SUM(HomePurchasesVat) AS HomePurchasesVat,
			SUM(ExportSalesVat) AS ExportSalesVat,
			SUM(ExportPurchasesVat) AS ExportPurchasesVat
		FROM vat_detail
		GROUP BY
			StartOn,
			TaxCode
	)
	SELECT
		StartOn,
		TaxCode,
		CAST(HomeSales AS float) AS HomeSales,
		CAST(HomePurchases AS float) AS HomePurchases,
		CAST(ExportSales AS float) AS ExportSales,
		CAST(ExportPurchases AS float) AS ExportPurchases,
		CAST(HomeSalesVat AS float) AS HomeSalesVat,
		CAST(HomePurchasesVat AS float) AS HomePurchasesVat,
		CAST(ExportSalesVat AS float) AS ExportSalesVat,
		CAST(ExportPurchasesVat AS float) AS ExportPurchasesVat,
		CAST((HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS float) AS VatDue
	FROM vatcode_summary;
GO
