CREATE VIEW Cash.vwFlowVatPeriodTotals
AS
	WITH active_periods AS
	(
		SELECT App.tbYear.YearNumber, App.tbYearPeriod.StartOn
		FROM            App.tbYearPeriod INNER JOIN
								 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
								 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
		WHERE        (App.tbYear.CashStatusCode < 3)
	)
	SELECT     active_periods.YearNumber, active_periods.StartOn,	
		CAST(ISNULL(SUM(vat.HomeSales), 0) as decimal(18, 5)) AS HomeSales, 
		CAST(ISNULL(SUM(vat.HomePurchases), 0) as decimal(18, 5)) AS HomePurchases, 
		CAST(ISNULL(SUM(vat.ExportSales), 0) as decimal(18, 5)) AS ExportSales, 
		CAST(ISNULL(SUM(vat.ExportPurchases), 0) as decimal(18, 5)) AS ExportPurchases, 
		CAST(ISNULL(SUM(vat.HomeSalesVat), 0) as decimal(18, 5)) AS HomeSalesVat, 
		CAST(ISNULL(SUM(vat.HomePurchasesVat), 0) as decimal(18, 5)) AS HomePurchasesVat, 
		CAST(ISNULL(SUM(vat.ExportSalesVat), 0) as decimal(18, 5)) AS ExportSalesVat, 
		CAST(ISNULL(SUM(vat.ExportPurchasesVat), 0) as decimal(18, 5)) AS ExportPurchasesVat, 
		CAST(ISNULL(SUM(vat.VatDue), 0) as decimal(18, 5)) AS VatDue
	FROM            active_periods LEFT OUTER JOIN
							 Cash.vwTaxVatSummary AS vat ON active_periods.StartOn = vat.StartOn
	GROUP BY active_periods.YearNumber, active_periods.StartOn;
