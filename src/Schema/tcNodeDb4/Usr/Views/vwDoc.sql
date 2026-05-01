CREATE VIEW Usr.vwDoc
AS
	WITH bank AS
	(
		SELECT TOP (1)
			(SELECT SubjectCode FROM App.tbOptions) AS SubjectCode,
			s.SubjectName AS BankName,
			a.AccountName AS CurrentAccountName,
			CONCAT(s.SubjectName, SPACE(1), a.AccountName) AS BankAccount,
			a.SortCode AS BankSortCode,
			a.AccountNumber AS BankAccountNumber,
			ba.Address AS BankAddress
		FROM Subject.tbAccount AS a
			INNER JOIN Subject.tbSubject AS s
				ON a.SubjectCode = s.SubjectCode
			LEFT OUTER JOIN Subject.tbAddress AS ba
				ON s.AddressCode = ba.AddressCode
		WHERE a.CashCode IS NOT NULL
			AND a.AccountTypeCode = 0
	)
	SELECT TOP (1)
		company.SubjectName AS CompanyName,
		addr.Address AS CompanyAddress,
		company.PhoneNumber AS CompanyPhoneNumber,
		company.EmailAddress AS CompanyEmailAddress,
		virtual.WebSite AS CompanyWebsite,
		virtual.CompanyNumber,
		virtual.VatNumber,
		virtual.Logo,
		bank_details.BankName,
		bank_details.CurrentAccountName,
		bank_details.BankAccount,
		bank_details.BankAccountNumber,
		bank_details.BankSortCode,
		bank_details.BankAddress
	FROM Subject.tbSubject AS company
		INNER JOIN App.tbOptions AS opt
			ON company.SubjectCode = opt.SubjectCode
		LEFT OUTER JOIN Subject.tbVirtual AS virtual
			ON company.SubjectCode = virtual.SubjectCode
		LEFT OUTER JOIN bank AS bank_details
			ON company.SubjectCode = bank_details.SubjectCode
		LEFT OUTER JOIN Subject.tbAddress AS addr
			ON company.AddressCode = addr.AddressCode;
GO
