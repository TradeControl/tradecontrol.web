CREATE VIEW Invoice.vwMirrorDetails
AS
	SELECT invoice_task.ContractAddress, invoice_task.TaskCode DetailRef, mirror.ActivityCode DetailCode, alloc.AllocationDescription DetailDescription,
		invoice_task.Quantity, invoice_task.InvoiceValue, invoice_task.TaxValue, invoice_task.TaxCode, invoice_task.RowVer 
	FROM Invoice.tbMirrorTask invoice_task
		JOIN Invoice.tbMirror invoice ON invoice.ContractAddress = invoice_task.ContractAddress
		JOIN Task.tbAllocation alloc ON alloc.AccountCode = invoice.AccountCode AND alloc.TaskCode = invoice_task.TaskCode
		JOIN Activity.tbMirror mirror ON alloc.AccountCode = mirror.AccountCode AND alloc.AllocationCode = mirror.AllocationCode
	UNION
	SELECT invoice_item.ContractAddress, invoice_item.ChargeCode DetailRef, mirror.CashCode DetailCode, invoice_item.ChargeDescription DetailDescription,
		0 Quantity, invoice_item.InvoiceValue, invoice_item.TaxValue, invoice_item.TaxCode, invoice_item.RowVer
	FROM Invoice.tbMirrorItem invoice_item
		JOIN Invoice.tbMirror invoice ON invoice.ContractAddress = invoice_item.ContractAddress
		JOIN Cash.tbMirror mirror ON invoice_item.ChargeCode = mirror.ChargeCode AND invoice.AccountCode = mirror.AccountCode;

