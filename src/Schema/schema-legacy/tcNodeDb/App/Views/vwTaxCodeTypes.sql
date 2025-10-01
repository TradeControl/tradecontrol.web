
CREATE   VIEW App.vwTaxCodeTypes
AS
SELECT        TaxTypeCode, TaxType
FROM            Cash.tbTaxType
WHERE        (TaxTypeCode > 0);
