CREATE VIEW Invoice.vwHistoryPurchaseItems
AS
	SELECT        CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, App.tbYearPeriod.YearNumber, Invoice.vwRegisterDetail.StartOn, Invoice.vwRegisterDetail.InvoiceNumber, 
							 Invoice.vwRegisterDetail.ProjectCode, Invoice.vwRegisterDetail.CashCode, Invoice.vwRegisterDetail.CashDescription, Invoice.vwRegisterDetail.TaxCode, Invoice.vwRegisterDetail.TaxDescription, 
							 Invoice.vwRegisterDetail.SubjectCode, Invoice.vwRegisterDetail.InvoiceTypeCode, Invoice.vwRegisterDetail.InvoiceStatusCode, Invoice.vwRegisterDetail.InvoicedOn, Invoice.vwRegisterDetail.InvoiceValue, 
							 Invoice.vwRegisterDetail.TaxValue, Invoice.vwRegisterDetail.PaymentTerms, Invoice.vwRegisterDetail.Printed, 
							 Invoice.vwRegisterDetail.SubjectName, Invoice.vwRegisterDetail.UserName, Invoice.vwRegisterDetail.InvoiceStatus, Invoice.vwRegisterDetail.CashPolarityCode, Invoice.vwRegisterDetail.InvoiceType
	FROM            Invoice.vwRegisterDetail INNER JOIN
							 App.tbYearPeriod ON Invoice.vwRegisterDetail.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE        (Invoice.vwRegisterDetail.InvoiceTypeCode > 1);
