CREATE VIEW Subject.vwDatasheet
AS
	WITH Project_count AS
	(
		SELECT
			SubjectCode,
			COUNT(ProjectCode) AS ProjectCount
		FROM Project.tbProject
		WHERE ProjectStatusCode = 1
		GROUP BY
			SubjectCode
	)
	SELECT
		s.SubjectCode,
		s.SubjectName,
		ISNULL(pc.ProjectCount, 0) AS Projects,
		s.SubjectTypeCode,
		t.SubjectType,
		t.CashPolarityCode,
		s.SubjectStatusCode,
		st.SubjectStatus,
		ts.TransmitStatus,
		a.Address,
		tc.TaxDescription,
		s.TaxCode,
		s.AddressCode,
		s.AreaCode,
		s.PhoneNumber,
		s.EmailAddress,
		v.WebSite,
		(
			SELECT TOP (1)
				sector.IndustrySector
			FROM Subject.tbSector AS sector
			WHERE sector.SubjectCode = s.SubjectCode
		) AS IndustrySector,
		v.SubjectSource,
		s.PaymentTerms,
		s.PaymentDays,
		s.ExpectedDays,
		s.PayDaysFromMonthEnd,
		s.PayBalance,
		ISNULL(v.NumberOfEmployees, 0) AS NumberOfEmployees,
		v.CompanyNumber,
		v.VatNumber,
		ISNULL(v.Turnover, 0) AS Turnover,
		s.OpeningBalance,
		ISNULL(v.EUJurisdiction, 0) AS EUJurisdiction,
		v.BusinessDescription,
		s.InsertedBy,
		s.InsertedOn,
		s.UpdatedBy,
		s.UpdatedOn
	FROM Subject.tbSubject AS s
		INNER JOIN Subject.tbStatus AS st
			ON s.SubjectStatusCode = st.SubjectStatusCode
		INNER JOIN Subject.tbType AS t
			ON s.SubjectTypeCode = t.SubjectTypeCode
		INNER JOIN Subject.tbTransmitStatus AS ts
			ON s.TransmitStatusCode = ts.TransmitStatusCode
		LEFT OUTER JOIN Subject.tbVirtual AS v
			ON s.SubjectCode = v.SubjectCode
		LEFT OUTER JOIN App.tbTaxCode AS tc
			ON s.TaxCode = tc.TaxCode
		LEFT OUTER JOIN Subject.tbAddress AS a
			ON s.AddressCode = a.AddressCode
		LEFT OUTER JOIN Project_count AS pc
			ON s.SubjectCode = pc.SubjectCode;
GO
