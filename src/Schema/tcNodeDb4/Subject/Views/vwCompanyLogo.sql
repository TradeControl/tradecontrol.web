CREATE VIEW Subject.vwCompanyLogo
AS
	SELECT TOP (1)
		v.Logo
	FROM App.tbOptions AS o
		INNER JOIN Subject.tbSubject AS s
			ON o.SubjectCode = s.SubjectCode
		LEFT OUTER JOIN Subject.tbVirtual AS v
			ON s.SubjectCode = v.SubjectCode;
GO
