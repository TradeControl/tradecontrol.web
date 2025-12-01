CREATE VIEW Cash.vwTaxVatTotals
AS
	WITH vat_dates AS
	(
		SELECT PayFrom, PayTo FROM Cash.fnTaxTypeDueDates(1)
	), vatPeriod AS
	(
		SELECT        StartOn, y.YearNumber, p.MonthNumber,
			(SELECT PayTo FROM vat_dates WHERE p.StartOn >= PayFrom AND p.StartOn < PayTo) AS VatStartOn, VatAdjustment
		FROM            App.tbYearPeriod AS p JOIN App.tbYear AS y ON p.YearNumber = y.YearNumber 
		WHERE     (y.CashStatusCode = 1) OR (y.CashStatusCode = 2)
	), vat_results AS
	(
		SELECT VatStartOn AS PayTo, DATEADD(MONTH, -1, VatStartOn) AS PostOn,
			SUM(HomeSales) AS HomeSales, SUM(HomePurchases) AS HomePurchases, SUM(ExportSales) AS ExportSales, SUM(ExportPurchases) AS ExportPurchases, 
			SUM(HomeSalesVat) AS HomeSalesVat, SUM(HomePurchasesVat) AS HomePurchasesVat, 
			SUM(ExportSalesVat) AS ExportSalesVat, SUM(ExportPurchasesVat) AS ExportPurchasesVat, SUM(VatDue) AS VatDue
		FROM Cash.vwTaxVatSummary vatCodeDue JOIN vatPeriod ON vatCodeDue.StartOn = vatPeriod.StartOn
		GROUP BY VatStartOn
	), vat_adjustments AS
	(
		SELECT VatStartOn AS PayTo, CAST(SUM(VatAdjustment) as float) AS VatAdjustment
		FROM vatPeriod p 
		GROUP BY VatStartOn
	)
	SELECT active_year.YearNumber, active_year.Description, active_month.MonthName AS Period, vat_results.PostOn AS StartOn, HomeSales, HomePurchases, ExportSales, ExportPurchases, HomeSalesVat, HomePurchasesVat, ExportSalesVat, ExportPurchasesVat,
		vat_adjustments.VatAdjustment, VatDue - vat_adjustments.VatAdjustment AS VatDue
	FROM vat_results JOIN vat_adjustments ON vat_results.PayTo = vat_adjustments.PayTo
		JOIN App.tbYearPeriod year_period ON vat_results.PostOn = year_period.StartOn
		JOIN App.tbMonth active_month ON year_period.MonthNumber = active_month.MonthNumber
		JOIN App.tbYear active_year ON year_period.YearNumber = active_year.YearNumber;
