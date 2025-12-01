CREATE VIEW Cash.vwTaxVatDetails
AS
SELECT        App.tbYearPeriod.YearNumber, App.tbYear.Description, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, Cash.vwTaxVatSummary.StartOn, 
                         Cash.vwTaxVatSummary.TaxCode, Cash.vwTaxVatSummary.HomeSales, Cash.vwTaxVatSummary.HomePurchases, Cash.vwTaxVatSummary.ExportSales, Cash.vwTaxVatSummary.ExportPurchases, 
                         Cash.vwTaxVatSummary.HomeSalesVat, Cash.vwTaxVatSummary.HomePurchasesVat, Cash.vwTaxVatSummary.ExportSalesVat, Cash.vwTaxVatSummary.ExportPurchasesVat, Cash.vwTaxVatSummary.VatDue                         
FROM            Cash.vwTaxVatSummary INNER JOIN
                         App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber ON Cash.vwTaxVatSummary.StartOn = App.tbYearPeriod.StartOn INNER JOIN
                         App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber;

