
CREATE   VIEW Cash.vwVATCodes
AS
SELECT        TaxCode, TaxDescription
FROM            App.tbTaxCode
WHERE        (TaxTypeCode = 1);
