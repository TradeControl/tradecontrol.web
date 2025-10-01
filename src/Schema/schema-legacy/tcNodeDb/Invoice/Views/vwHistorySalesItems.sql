CREATE VIEW Invoice.vwHistorySalesItems
AS
	SELECT        App.tbYearPeriod.YearNumber, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, Invoice.vwRegisterDetail.StartOn, Invoice.vwRegisterDetail.InvoiceNumber, 
							 Invoice.vwRegisterDetail.TaskCode, Invoice.vwRegisterDetail.CashCode, Invoice.vwRegisterDetail.CashDescription, Invoice.vwRegisterDetail.TaxCode, Invoice.vwRegisterDetail.AccountCode, Invoice.vwRegisterDetail.InvoicedOn, 
							 Invoice.vwRegisterDetail.InvoiceValue, Invoice.vwRegisterDetail.TaxValue, Invoice.vwRegisterDetail.PaymentTerms, 
							 Invoice.vwRegisterDetail.AccountName, Invoice.vwRegisterDetail.InvoiceStatus, Invoice.vwRegisterDetail.InvoiceType, Invoice.vwRegisterDetail.InvoiceTypeCode, 
							 Invoice.vwRegisterDetail.InvoiceStatusCode
	FROM            Invoice.vwRegisterDetail INNER JOIN
							 App.tbYearPeriod ON Invoice.vwRegisterDetail.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE        (Invoice.vwRegisterDetail.InvoiceTypeCode < 2);
