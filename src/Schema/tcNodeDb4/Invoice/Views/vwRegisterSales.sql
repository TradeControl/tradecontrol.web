CREATE    VIEW Invoice.vwRegisterSales
AS
SELECT        StartOn, InvoiceNumber, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, PaymentTerms, Notes, Printed, SubjectName, UserName, 
                         InvoiceStatus, CashPolarityCode, InvoiceType, (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) AS UnpaidValue
FROM            Invoice.vwRegister
WHERE        (InvoiceTypeCode < 2);
