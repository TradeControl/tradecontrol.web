CREATE VIEW Invoice.vwItems
AS
SELECT        Invoice.tbItem.InvoiceNumber, Invoice.tbItem.CashCode, Cash.tbCode.CashDescription, Invoice.tbItem.TaxCode, Invoice.tbItem.TaxValue, Invoice.tbItem.InvoiceValue, Invoice.tbItem.ItemReference, 
                         Invoice.tbInvoice.InvoicedOn
FROM            Invoice.tbItem INNER JOIN
                         Invoice.tbInvoice ON Invoice.tbItem.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
                         Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode;

