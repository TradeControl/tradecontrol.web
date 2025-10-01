CREATE   VIEW Subject.vwListAll
AS
	WITH accounts AS
	(
		SELECT SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode, TaxCode,
			(SELECT TOP 1 CashCode FROM Project.tbProject WHERE SubjectCode = Subjects.SubjectCode ORDER BY ActionOn DESC) ProjectCashCode,
			(SELECT TOP 1 CashCode FROM Cash.tbPayment WHERE SubjectCode = Subjects.SubjectCode ORDER BY PaidOn DESC) PaymentCashCode
		FROM  Subject.tbSubject Subjects
	)
		SELECT accounts.SubjectCode, accounts.SubjectName, Subject_type.SubjectType, accounts.TaxCode, Subject_type.CashPolarityCode, accounts.SubjectStatusCode,
			COALESCE(accounts.ProjectCashCode, accounts.PaymentCashCode) CashCode
		FROM accounts 
			INNER JOIN Subject.tbType AS Subject_type ON accounts.SubjectTypeCode = Subject_type.SubjectTypeCode
