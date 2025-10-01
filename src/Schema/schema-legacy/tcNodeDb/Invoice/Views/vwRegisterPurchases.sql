CREATE   VIEW Invoice.vwRegisterPurchases
AS
SELECT        StartOn, InvoiceNumber, AccountCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, PaymentTerms, Notes, Printed, AccountName, UserName, 
                         InvoiceStatus, CashModeCode, InvoiceType, (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) AS UnpaidValue
FROM            Invoice.vwRegister
WHERE        (InvoiceTypeCode > 1);
