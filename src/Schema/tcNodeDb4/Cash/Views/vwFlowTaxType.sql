
CREATE   VIEW Cash.vwFlowTaxType
AS
	SELECT       Cash.tbTaxType.TaxTypeCode, Cash.tbTaxType.TaxType, Cash.tbTaxType.RecurrenceCode, App.tbRecurrence.Recurrence, Cash.tbTaxType.CashCode, Cash.tbCode.CashDescription, Cash.tbTaxType.MonthNumber, App.tbMonth.MonthName, Cash.tbTaxType.SubjectCode, 
								Cash.tbTaxType.OffsetDays
	FROM            Cash.tbTaxType INNER JOIN
								App.tbRecurrence ON Cash.tbTaxType.RecurrenceCode = App.tbRecurrence.RecurrenceCode INNER JOIN
								Cash.tbCode ON Cash.tbTaxType.CashCode = Cash.tbCode.CashCode INNER JOIN
								App.tbMonth ON Cash.tbTaxType.MonthNumber = App.tbMonth.MonthNumber
