CREATE VIEW Usr.vwDoc
AS
	WITH bank AS 
	(
		SELECT TOP (1) (SELECT SubjectCode FROM App.tbOptions) AS SubjectCode, 
			Subject.tbSubject.SubjectName AS BankName,
			Subject.tbAccount.AccountName AS CurrentAccountName,
			CONCAT(Subject.tbSubject.SubjectName, SPACE(1), Subject.tbAccount.AccountName) AS BankAccount, 
			Subject.tbAccount.SortCode AS BankSortCode, Subject.tbAccount.AccountNumber AS BankAccountNumber
		FROM Subject.tbAccount 
			INNER JOIN Subject.tbSubject ON Subject.tbAccount.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE (NOT (Subject.tbAccount.CashCode IS NULL)) AND (Subject.tbAccount.AccountTypeCode = 0)
	)
    SELECT        TOP (1) company.SubjectName AS CompanyName, Subject.tbAddress.Address AS CompanyAddress, company.PhoneNumber AS CompanyPhoneNumber,  
                              company.EmailAddress AS CompanyEmailAddress, company.WebSite AS CompanyWebsite, company.CompanyNumber, company.VatNumber, company.Logo, 
							  bank_details.BankName, bank_details.CurrentAccountName,
							  bank_details.BankAccount, bank_details.BankAccountNumber, bank_details.BankSortCode
     FROM            Subject.tbSubject AS company INNER JOIN
                              App.tbOptions ON company.SubjectCode = App.tbOptions.SubjectCode LEFT OUTER JOIN
                              bank AS bank_details ON company.SubjectCode = bank_details.SubjectCode LEFT OUTER JOIN
                              Subject.tbAddress ON company.AddressCode = Subject.tbAddress.AddressCode;

