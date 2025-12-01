CREATE   VIEW Web.vwAttachmentInvoices
AS
	SELECT Web.tbAttachmentInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Web.tbAttachmentInvoice.AttachmentId, Web.tbAttachment.AttachmentFileName
	FROM Web.tbAttachmentInvoice 
		JOIN Invoice.tbType ON Web.tbAttachmentInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode 
		JOIN Web.tbAttachment ON Web.tbAttachmentInvoice.AttachmentId = Web.tbAttachment.AttachmentId;
