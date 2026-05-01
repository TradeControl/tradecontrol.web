CREATE VIEW Subject.vwVirtual
AS
    SELECT
	    s.SubjectCode,
	    s.SubjectName,
	    s.SubjectTypeCode,
	    s.SubjectStatusCode,
	    s.TransmitStatusCode,
	    s.TaxCode,
	    s.AddressCode,
	    s.PaymentTerms,
	    s.ExpectedDays,
	    s.PaymentDays,
	    s.PayDaysFromMonthEnd,
	    s.PayBalance,
	    s.OpeningBalance,
	    s.AreaCode,
	    s.PhoneNumber,
	    s.EmailAddress,
	    s.InsertedBy,
	    s.InsertedOn,
	    s.UpdatedBy,
	    s.UpdatedOn,
	    v.NumberOfEmployees,
	    v.CompanyNumber,
	    v.VatNumber,
	    v.EUJurisdiction,
	    v.BusinessDescription,
	    v.Logo,
	    v.Turnover,
	    v.WebSite,
	    v.SubjectSource,
	    Subject.fnSubjectNamespace(s.SubjectCode) AS SubjectNamespace
    FROM Subject.tbSubject AS s
	    JOIN Subject.tbVirtual AS v
		    ON v.SubjectCode = s.SubjectCode
	    JOIN Subject.tbType AS t
		    ON t.SubjectTypeCode = s.SubjectTypeCode
    WHERE
	    t.SubjectClassCode = 0;
GO
