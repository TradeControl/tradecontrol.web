CREATE VIEW Usr.vwDoc
AS
	WITH bank AS 
	(
		SELECT TOP (1) (SELECT AccountCode FROM App.tbOptions) AS AccountCode, 
			Org.tbOrg.AccountName AS BankName,
			Org.tbAccount.CashAccountName AS CurrentAccountName,
			CONCAT(Org.tbOrg.AccountName, SPACE(1), Org.tbAccount.CashAccountName) AS BankAccount, 
			Org.tbAccount.SortCode AS BankSortCode, Org.tbAccount.AccountNumber AS BankAccountNumber
		FROM Org.tbAccount 
			INNER JOIN Org.tbOrg ON Org.tbAccount.AccountCode = Org.tbOrg.AccountCode
		WHERE (NOT (Org.tbAccount.CashCode IS NULL)) AND (Org.tbAccount.AccountTypeCode = 0)
	)
    SELECT        TOP (1) company.AccountName AS CompanyName, Org.tbAddress.Address AS CompanyAddress, company.PhoneNumber AS CompanyPhoneNumber,  
                              company.EmailAddress AS CompanyEmailAddress, company.WebSite AS CompanyWebsite, company.CompanyNumber, company.VatNumber, company.Logo, 
							  bank_details.BankName, bank_details.CurrentAccountName,
							  bank_details.BankAccount, bank_details.BankAccountNumber, bank_details.BankSortCode
     FROM            Org.tbOrg AS company INNER JOIN
                              App.tbOptions ON company.AccountCode = App.tbOptions.AccountCode LEFT OUTER JOIN
                              bank AS bank_details ON company.AccountCode = bank_details.AccountCode LEFT OUTER JOIN
                              Org.tbAddress ON company.AddressCode = Org.tbAddress.AddressCode;

