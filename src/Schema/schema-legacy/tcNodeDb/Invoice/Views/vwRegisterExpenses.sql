CREATE VIEW Invoice.vwRegisterExpenses
 AS
	SELECT     Invoice.vwRegisterTasks.StartOn, Invoice.vwRegisterTasks.InvoiceNumber, Invoice.vwRegisterTasks.TaskCode, App.tbYearPeriod.YearNumber, 
						  App.tbYear.Description, App.tbMonth.MonthName + ' ' + LTRIM(STR(YEAR( App.tbYearPeriod.StartOn))) AS Period, Invoice.vwRegisterTasks.TaskTitle, 
						  Invoice.vwRegisterTasks.CashCode, Invoice.vwRegisterTasks.CashDescription, Invoice.vwRegisterTasks.TaxCode, Invoice.vwRegisterTasks.TaxDescription, 
						  Invoice.vwRegisterTasks.AccountCode, Invoice.vwRegisterTasks.InvoiceTypeCode, Invoice.vwRegisterTasks.InvoiceStatusCode, Invoice.vwRegisterTasks.InvoicedOn, 
						  Invoice.vwRegisterTasks.InvoiceValue, Invoice.vwRegisterTasks.TaxValue, 
						  Invoice.vwRegisterTasks.PaymentTerms, Invoice.vwRegisterTasks.Printed, Invoice.vwRegisterTasks.AccountName, Invoice.vwRegisterTasks.UserName, 
						  Invoice.vwRegisterTasks.InvoiceStatus, Invoice.vwRegisterTasks.CashModeCode, Invoice.vwRegisterTasks.InvoiceType
	FROM         Invoice.vwRegisterTasks INNER JOIN
						  App.tbYearPeriod ON Invoice.vwRegisterTasks.StartOn = App.tbYearPeriod.StartOn INNER JOIN
						  App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
						  App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE     (Task.fnIsExpense(Invoice.vwRegisterTasks.TaskCode) = 1)
