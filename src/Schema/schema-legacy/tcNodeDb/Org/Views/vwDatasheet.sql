CREATE VIEW Org.vwDatasheet
AS
	With task_count AS
	(
		SELECT        AccountCode, COUNT(TaskCode) AS TaskCount
		FROM            Task.tbTask
		WHERE        (TaskStatusCode = 1)
		GROUP BY AccountCode
	)
	SELECT        o.AccountCode, o.AccountName, ISNULL(task_count.TaskCount, 0) AS Tasks, o.OrganisationTypeCode, Org.tbType.OrganisationType, Org.tbType.CashModeCode, o.OrganisationStatusCode, 
							 Org.tbStatus.OrganisationStatus, Org.tbTransmitStatus.TransmitStatus, Org.tbAddress.Address, App.tbTaxCode.TaxDescription, o.TaxCode, o.AddressCode, o.AreaCode, o.PhoneNumber, o.EmailAddress, o.WebSite,
								 (SELECT        TOP (1) IndustrySector
								   FROM            Org.tbSector AS sector
								   WHERE        (AccountCode = o.AccountCode)) AS IndustrySector, o.AccountSource, o.PaymentTerms, o.PaymentDays, o.ExpectedDays, o.PayDaysFromMonthEnd, o.PayBalance, o.NumberOfEmployees, o.CompanyNumber, o.VatNumber, o.Turnover, 
							 o.OpeningBalance, o.EUJurisdiction, o.BusinessDescription, o.InsertedBy, o.InsertedOn, o.UpdatedBy, o.UpdatedOn 
	FROM            Org.tbOrg AS o 
		JOIN Org.tbStatus ON o.OrganisationStatusCode = Org.tbStatus.OrganisationStatusCode 
		JOIN Org.tbType ON o.OrganisationTypeCode = Org.tbType.OrganisationTypeCode 
		JOIN Org.tbTransmitStatus ON o.TransmitStatusCode = Org.tbTransmitStatus.TransmitStatusCode
		LEFT OUTER JOIN App.tbTaxCode ON o.TaxCode = App.tbTaxCode.TaxCode 
		LEFT OUTER JOIN Org.tbAddress ON o.AddressCode = Org.tbAddress.AddressCode 
		LEFT OUTER JOIN task_count ON o.AccountCode = task_count.AccountCode
