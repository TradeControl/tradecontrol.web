CREATE VIEW Invoice.vwRegisterDetail
AS
	WITH register AS
	(
		SELECT     StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, 
							  InvoicedOn, DueOn, ExpectedOn, CAST(Quantity as float) Quantity, CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, PaymentTerms, Printed, SubjectName, UserName, UserId, InvoiceStatus, CashPolarityCode, 
							  InvoiceType, CAST(1 as bit) IsProject, NULL ItemReference
		FROM         Invoice.vwRegisterProjects
		UNION
		SELECT     StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, 
							  InvoicedOn, DueOn, ExpectedOn, CAST(0 as float) Quantity, CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, PaymentTerms, Printed, SubjectName, UserName, UserId, InvoiceStatus, CashPolarityCode, 
							  InvoiceType, CAST(0 as bit) IsProject, ItemReference
		FROM         Invoice.vwRegisterItems
	)
	SELECT StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, 
		InvoicedOn, DueOn, ExpectedOn, PaymentTerms, Printed, SubjectName, UserName, UserId, InvoiceStatus, CashPolarityCode, InvoiceType,
		Quantity, InvoiceValue, TaxValue, (InvoiceValue + TaxValue) TotalValue, IsProject, ItemReference
	FROM register;
