CREATE VIEW Subject.vwDatasheet
AS
	With Project_count AS
	(
		SELECT        SubjectCode, COUNT(ProjectCode) AS ProjectCount
		FROM            Project.tbProject
		WHERE        (ProjectStatusCode = 1)
		GROUP BY SubjectCode
	)
	SELECT        o.SubjectCode, o.SubjectName, ISNULL(Project_count.ProjectCount, 0) AS Projects, o.SubjectTypeCode, Subject.tbType.SubjectType, Subject.tbType.CashPolarityCode, o.SubjectStatusCode, 
							 Subject.tbStatus.SubjectStatus, Subject.tbTransmitStatus.TransmitStatus, Subject.tbAddress.Address, App.tbTaxCode.TaxDescription, o.TaxCode, o.AddressCode, o.AreaCode, o.PhoneNumber, o.EmailAddress, o.WebSite,
								 (SELECT        TOP (1) IndustrySector
								   FROM            Subject.tbSector AS sector
								   WHERE        (SubjectCode = o.SubjectCode)) AS IndustrySector, o.SubjectSource, o.PaymentTerms, o.PaymentDays, o.ExpectedDays, o.PayDaysFromMonthEnd, o.PayBalance, o.NumberOfEmployees, o.CompanyNumber, o.VatNumber, o.Turnover, 
							 o.OpeningBalance, o.EUJurisdiction, o.BusinessDescription, o.InsertedBy, o.InsertedOn, o.UpdatedBy, o.UpdatedOn 
	FROM            Subject.tbSubject AS o 
		JOIN Subject.tbStatus ON o.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode 
		JOIN Subject.tbType ON o.SubjectTypeCode = Subject.tbType.SubjectTypeCode 
		JOIN Subject.tbTransmitStatus ON o.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode
		LEFT OUTER JOIN App.tbTaxCode ON o.TaxCode = App.tbTaxCode.TaxCode 
		LEFT OUTER JOIN Subject.tbAddress ON o.AddressCode = Subject.tbAddress.AddressCode 
		LEFT OUTER JOIN Project_count ON o.SubjectCode = Project_count.SubjectCode
