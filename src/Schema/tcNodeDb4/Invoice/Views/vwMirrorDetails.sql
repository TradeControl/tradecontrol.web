CREATE VIEW Invoice.vwMirrorDetails
AS
	SELECT invoice_Project.ContractAddress, invoice_Project.ProjectCode DetailRef, mirror.ObjectCode DetailCode, alloc.AllocationDescription DetailDescription,
		invoice_Project.Quantity, invoice_Project.InvoiceValue, invoice_Project.TaxValue, invoice_Project.TaxCode, invoice_Project.RowVer 
	FROM Invoice.tbMirrorProject invoice_Project
		JOIN Invoice.tbMirror invoice ON invoice.ContractAddress = invoice_Project.ContractAddress
		JOIN Project.tbAllocation alloc ON alloc.SubjectCode = invoice.SubjectCode AND alloc.ProjectCode = invoice_Project.ProjectCode
		JOIN Object.tbMirror mirror ON alloc.SubjectCode = mirror.SubjectCode AND alloc.AllocationCode = mirror.AllocationCode
	UNION
	SELECT invoice_item.ContractAddress, invoice_item.ChargeCode DetailRef, mirror.CashCode DetailCode, invoice_item.ChargeDescription DetailDescription,
		0 Quantity, invoice_item.InvoiceValue, invoice_item.TaxValue, invoice_item.TaxCode, invoice_item.RowVer
	FROM Invoice.tbMirrorItem invoice_item
		JOIN Invoice.tbMirror invoice ON invoice.ContractAddress = invoice_item.ContractAddress
		JOIN Cash.tbMirror mirror ON invoice_item.ChargeCode = mirror.ChargeCode AND invoice.SubjectCode = mirror.SubjectCode;

