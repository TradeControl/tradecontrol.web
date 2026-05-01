CREATE VIEW Cash.vwTaxVatAccruals
AS
	WITH Project_invoiced_quantity AS
	(
		SELECT
			ip.ProjectCode,
			SUM(ip.Quantity) AS InvoiceQuantity
		FROM Invoice.tbProject AS ip
			INNER JOIN Invoice.tbInvoice AS i
				ON ip.InvoiceNumber = i.InvoiceNumber
		WHERE i.InvoiceTypeCode IN (0, 2)
		GROUP BY
			ip.ProjectCode
	),
	Project_transactions AS
	(
		SELECT
			(
				SELECT TOP (1) yp.StartOn
				FROM App.tbYearPeriod AS yp
				WHERE yp.StartOn <= p.ActionOn
				ORDER BY yp.StartOn DESC
			) AS StartOn,
			p.ProjectCode,
			p.TaxCode,
			p.Quantity - ISNULL(iq.InvoiceQuantity, 0) AS QuantityRemaining,
			p.UnitCharge * (p.Quantity - ISNULL(iq.InvoiceQuantity, 0)) AS TotalValue,
			p.UnitCharge * (p.Quantity - ISNULL(iq.InvoiceQuantity, 0)) * tc.TaxRate AS TaxValue,
			tc.TaxRate,
			ISNULL(v.EUJurisdiction, 0) AS EUJurisdiction,
			cat.CashPolarityCode
		FROM Project.tbProject AS p
			INNER JOIN Subject.tbSubject AS s
				ON p.SubjectCode = s.SubjectCode
			LEFT OUTER JOIN Subject.tbVirtual AS v
				ON s.SubjectCode = v.SubjectCode
			INNER JOIN Cash.tbCode AS c
				ON p.CashCode = c.CashCode
			INNER JOIN Cash.tbCategory AS cat
				ON c.CategoryCode = cat.CategoryCode
			INNER JOIN App.tbTaxCode AS tc
				ON p.TaxCode = tc.TaxCode
			LEFT OUTER JOIN Project_invoiced_quantity AS iq
				ON p.ProjectCode = iq.ProjectCode
		WHERE tc.TaxTypeCode = 1
			AND p.ProjectStatusCode > 0
			AND p.ProjectStatusCode < 3
			AND p.ActionOn <= (SELECT DATEADD(DAY, TaxHorizon, CURRENT_TIMESTAMP) FROM App.tbOptions)
	),
	Project_dataset AS
	(
		SELECT
			StartOn,
			ProjectCode,
			TaxCode,
			QuantityRemaining,
			TotalValue,
			TaxValue,
			TaxRate,
			CAST(CASE WHEN EUJurisdiction = 0 THEN CASE CashPolarityCode WHEN 1 THEN TotalValue ELSE 0 END ELSE 0 END AS float) AS HomeSales,
			CAST(CASE WHEN EUJurisdiction = 0 THEN CASE CashPolarityCode WHEN 0 THEN TotalValue ELSE 0 END ELSE 0 END AS float) AS HomePurchases,
			CAST(CASE WHEN EUJurisdiction <> 0 THEN CASE CashPolarityCode WHEN 1 THEN TotalValue ELSE 0 END ELSE 0 END AS float) AS ExportSales,
			CAST(CASE WHEN EUJurisdiction <> 0 THEN CASE CashPolarityCode WHEN 0 THEN TotalValue ELSE 0 END ELSE 0 END AS float) AS ExportPurchases,
			CAST(CASE WHEN EUJurisdiction = 0 THEN CASE CashPolarityCode WHEN 1 THEN TaxValue ELSE 0 END ELSE 0 END AS float) AS HomeSalesVat,
			CAST(CASE WHEN EUJurisdiction = 0 THEN CASE CashPolarityCode WHEN 0 THEN TaxValue ELSE 0 END ELSE 0 END AS float) AS HomePurchasesVat,
			CAST(CASE WHEN EUJurisdiction <> 0 THEN CASE CashPolarityCode WHEN 1 THEN TaxValue ELSE 0 END ELSE 0 END AS float) AS ExportSalesVat,
			CAST(CASE WHEN EUJurisdiction <> 0 THEN CASE CashPolarityCode WHEN 0 THEN TaxValue ELSE 0 END ELSE 0 END AS float) AS ExportPurchasesVat
		FROM Project_transactions
	)
	SELECT
		pd.*,
		(HomeSalesVat + ExportSalesVat) - (HomePurchasesVat + ExportPurchasesVat) AS VatDue
	FROM Project_dataset AS pd
		INNER JOIN App.tbYearPeriod AS yp
			ON pd.StartOn = yp.StartOn
		INNER JOIN App.tbYear AS y
			ON yp.YearNumber = y.YearNumber
		INNER JOIN App.tbMonth AS m
			ON yp.MonthNumber = m.MonthNumber;
GO
