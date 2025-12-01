CREATE VIEW Cash.vwBudget
AS
SELECT TOP 100 PERCENT Cash.tbCode.CategoryCode, Cash.tbPeriod.CashCode, Cash.tbCode.CashDescription, 
	Cash.tbPeriod.StartOn, App.tbYearPeriod.YearNumber, App.tbMonth.MonthName, Format(App.tbYearPeriod.StartOn, 'yy-MM') AS Period,  
	Cash.tbPeriod.ForecastValue, Cash.tbPeriod.ForecastTax, Cash.tbPeriod.InvoiceValue, Cash.tbPeriod.InvoiceTax, Cash.tbPeriod.Note, Cash.tbPolarity.CashPolarity, App.tbTaxCode.TaxRate
FROM            App.tbYearPeriod INNER JOIN
                         Cash.tbPeriod ON App.tbYearPeriod.StartOn = Cash.tbPeriod.StartOn INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
                         Cash.tbCode ON Cash.tbPeriod.CashCode = Cash.tbCode.CashCode INNER JOIN
                         Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
						 Cash.tbPolarity ON Cash.tbCategory.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
                         App.tbTaxCode ON Cash.tbCode.TaxCode = App.tbTaxCode.TaxCode


