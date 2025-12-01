CREATE   VIEW Web.vwTemplateInvoices
AS
	SELECT Web.tbTemplateInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Web.tbTemplateInvoice.TemplateId, Web.tbTemplate.TemplateFileName, Web.tbTemplateInvoice.LastUsedOn
	FROM Web.tbTemplateInvoice 
		JOIN Invoice.tbType ON Web.tbTemplateInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode 
		JOIN Web.tbTemplate ON Web.tbTemplateInvoice.TemplateId = Web.tbTemplate.TemplateId;
