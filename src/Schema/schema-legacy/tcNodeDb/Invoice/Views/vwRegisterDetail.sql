CREATE VIEW Invoice.vwRegisterDetail
AS
	WITH register AS
	(
		SELECT     StartOn, InvoiceNumber, TaskCode, CashCode, CashDescription, TaxCode, TaxDescription, AccountCode, InvoiceTypeCode, InvoiceStatusCode, 
							  InvoicedOn, DueOn, ExpectedOn, CAST(Quantity as float) Quantity, CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, PaymentTerms, Printed, AccountName, UserName, UserId, InvoiceStatus, CashModeCode, 
							  InvoiceType, CAST(1 as bit) IsTask, NULL ItemReference
		FROM         Invoice.vwRegisterTasks
		UNION
		SELECT     StartOn, InvoiceNumber, TaskCode, CashCode, CashDescription, TaxCode, TaxDescription, AccountCode, InvoiceTypeCode, InvoiceStatusCode, 
							  InvoicedOn, DueOn, ExpectedOn, CAST(0 as float) Quantity, CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, PaymentTerms, Printed, AccountName, UserName, UserId, InvoiceStatus, CashModeCode, 
							  InvoiceType, CAST(0 as bit) IsTask, ItemReference
		FROM         Invoice.vwRegisterItems
	)
	SELECT StartOn, InvoiceNumber, TaskCode, CashCode, CashDescription, TaxCode, TaxDescription, AccountCode, InvoiceTypeCode, InvoiceStatusCode, 
		InvoicedOn, DueOn, ExpectedOn, PaymentTerms, Printed, AccountName, UserName, UserId, InvoiceStatus, CashModeCode, InvoiceType,
		Quantity, InvoiceValue, TaxValue, (InvoiceValue + TaxValue) TotalValue, IsTask, ItemReference
	FROM register;
