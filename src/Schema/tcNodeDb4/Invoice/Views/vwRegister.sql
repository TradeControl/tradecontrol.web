CREATE VIEW Invoice.vwRegister
AS
	WITH register AS 
	(
		SELECT       (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
				Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.SubjectCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
								 Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbInvoice.InvoiceValue * - 1 ELSE Invoice.tbInvoice.InvoiceValue END AS InvoiceValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbInvoice.TaxValue * - 1 ELSE Invoice.tbInvoice.TaxValue END AS TaxValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbInvoice.PaidValue * - 1 ELSE Invoice.tbInvoice.PaidValue END AS PaidValue, 
								 CASE WHEN Invoice.tbType.CashPolarityCode = 0 THEN Invoice.tbInvoice.PaidTaxValue * - 1 ELSE Invoice.tbInvoice.PaidTaxValue END AS PaidTaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, 
								 Invoice.tbInvoice.Printed, Subject.tbSubject.SubjectName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashPolarityCode, Invoice.tbType.InvoiceType
		FROM            Invoice.tbInvoice INNER JOIN
								 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
								 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
								 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
	)
	SELECT COALESCE(StartOn, CAST(getdate() as date)) StartOn, InvoiceNumber, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, DueOn, ExpectedOn,
		CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, CAST((InvoiceValue + TaxValue) as float) TotalInvoiceValue, 
		CAST(PaidValue as float) PaidValue, CAST(PaidTaxValue as float) PaidTaxValue, CAST((PaidValue + PaidTaxValue) as float) TotalPaidValue,
		PaymentTerms, Notes, Printed, SubjectName, UserName, UserId, InvoiceStatus, CashPolarityCode, InvoiceType
	FROM register;
