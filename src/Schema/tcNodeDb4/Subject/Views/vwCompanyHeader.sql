CREATE VIEW Subject.vwCompanyHeader
AS
	SELECT TOP (1)
		s.SubjectName AS CompanyName,
		a.Address AS CompanyAddress,
		s.PhoneNumber AS CompanyPhoneNumber,
		s.EmailAddress AS CompanyEmailAddress,
		v.WebSite AS CompanyWebsite,
		v.CompanyNumber,
		v.VatNumber
	FROM App.tbOptions AS o
		INNER JOIN Subject.tbSubject AS s
			ON o.SubjectCode = s.SubjectCode
		LEFT OUTER JOIN Subject.tbVirtual AS v
			ON s.SubjectCode = v.SubjectCode
		LEFT OUTER JOIN Subject.tbAddress AS a
			ON s.AddressCode = a.AddressCode;
GO
