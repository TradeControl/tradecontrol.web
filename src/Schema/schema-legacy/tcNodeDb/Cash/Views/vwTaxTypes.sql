CREATE   VIEW Cash.vwTaxTypes
AS
	SELECT        Cash.tbTaxType.TaxTypeCode, Cash.tbTaxType.TaxType, Cash.tbTaxType.CashCode, Cash.tbCode.CashDescription, Cash.tbTaxType.MonthNumber, App.tbMonth.[MonthName], Cash.tbTaxType.RecurrenceCode, 
							 App.tbRecurrence.Recurrence, Cash.tbTaxType.AccountCode, Org.tbOrg.AccountName, Cash.tbTaxType.OffsetDays
	FROM            Cash.tbTaxType INNER JOIN
							 Cash.tbCode ON Cash.tbTaxType.CashCode = Cash.tbCode.CashCode INNER JOIN
							 App.tbMonth ON Cash.tbTaxType.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							 App.tbRecurrence ON Cash.tbTaxType.RecurrenceCode = App.tbRecurrence.RecurrenceCode INNER JOIN
							 Org.tbOrg ON Cash.tbTaxType.AccountCode = Org.tbOrg.AccountCode;
