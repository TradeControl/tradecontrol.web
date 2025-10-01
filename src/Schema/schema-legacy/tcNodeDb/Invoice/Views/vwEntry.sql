CREATE   VIEW Invoice.vwEntry
AS
	SELECT        Invoice.tbEntry.UserId, Usr.tbUser.UserName, Invoice.tbEntry.AccountCode, Org.tbOrg.AccountName, Invoice.tbEntry.CashCode, Cash.tbCode.CashDescription, Invoice.tbEntry.InvoiceTypeCode, Invoice.tbType.InvoiceType, 
							 Invoice.tbEntry.InvoicedOn, Invoice.tbEntry.TaxCode, App.tbTaxCode.TaxDescription, Cash.tbTaxType.TaxType, Invoice.tbEntry.ItemReference, Invoice.tbEntry.TotalValue, Invoice.tbEntry.InvoiceValue, 
							 Invoice.tbEntry.InvoiceValue + Invoice.tbEntry.TotalValue AS EntryValue
	FROM            Invoice.tbEntry INNER JOIN
							 Org.tbOrg ON Invoice.tbEntry.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 Cash.tbCode ON Invoice.tbEntry.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Invoice.tbType ON Invoice.tbEntry.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Usr.tbUser ON Invoice.tbEntry.UserId = Usr.tbUser.UserId INNER JOIN
							 App.tbTaxCode ON Invoice.tbEntry.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Cash.tbTaxType ON App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND 
							 App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode
