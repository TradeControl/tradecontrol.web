CREATE VIEW App.vwTaxCodes
AS
	SELECT        App.tbTaxCode.TaxCode, App.tbTaxCode.TaxDescription, Cash.tbTaxType.TaxType, App.tbTaxCode.TaxTypeCode, App.tbTaxCode.RoundingCode, App.tbRounding.Rounding, App.tbTaxCode.TaxRate, App.tbTaxCode.Decimals, 
							 App.tbTaxCode.UpdatedBy, App.tbTaxCode.UpdatedOn
	FROM            App.tbTaxCode INNER JOIN
							 Cash.tbTaxType ON App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode INNER JOIN
							 App.tbRounding ON App.tbTaxCode.RoundingCode = App.tbRounding.RoundingCode

