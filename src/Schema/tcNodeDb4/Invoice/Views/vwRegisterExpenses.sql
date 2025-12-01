CREATE VIEW Invoice.vwRegisterExpenses
 AS
	SELECT     Invoice.vwRegisterProjects.StartOn, Invoice.vwRegisterProjects.InvoiceNumber, Invoice.vwRegisterProjects.ProjectCode, App.tbYearPeriod.YearNumber, 
						  App.tbYear.Description, App.tbMonth.MonthName + ' ' + LTRIM(STR(YEAR( App.tbYearPeriod.StartOn))) AS Period, Invoice.vwRegisterProjects.ProjectTitle, 
						  Invoice.vwRegisterProjects.CashCode, Invoice.vwRegisterProjects.CashDescription, Invoice.vwRegisterProjects.TaxCode, Invoice.vwRegisterProjects.TaxDescription, 
						  Invoice.vwRegisterProjects.SubjectCode, Invoice.vwRegisterProjects.InvoiceTypeCode, Invoice.vwRegisterProjects.InvoiceStatusCode, Invoice.vwRegisterProjects.InvoicedOn, 
						  Invoice.vwRegisterProjects.InvoiceValue, Invoice.vwRegisterProjects.TaxValue, 
						  Invoice.vwRegisterProjects.PaymentTerms, Invoice.vwRegisterProjects.Printed, Invoice.vwRegisterProjects.SubjectName, Invoice.vwRegisterProjects.UserName, 
						  Invoice.vwRegisterProjects.InvoiceStatus, Invoice.vwRegisterProjects.CashPolarityCode, Invoice.vwRegisterProjects.InvoiceType
	FROM         Invoice.vwRegisterProjects INNER JOIN
						  App.tbYearPeriod ON Invoice.vwRegisterProjects.StartOn = App.tbYearPeriod.StartOn INNER JOIN
						  App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
						  App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
	WHERE     (Project.fnIsExpense(Invoice.vwRegisterProjects.ProjectCode) = 1)
