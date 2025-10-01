CREATE   VIEW Org.vwListAll
AS
	WITH accounts AS
	(
		SELECT AccountCode, AccountName, OrganisationTypeCode, OrganisationStatusCode, TaxCode,
			(SELECT TOP 1 CashCode FROM Task.tbTask WHERE AccountCode = orgs.AccountCode ORDER BY ActionOn DESC) TaskCashCode,
			(SELECT TOP 1 CashCode FROM Cash.tbPayment WHERE AccountCode = orgs.AccountCode ORDER BY PaidOn DESC) PaymentCashCode
		FROM  Org.tbOrg orgs
	)
		SELECT accounts.AccountCode, accounts.AccountName, org_type.OrganisationType, accounts.TaxCode, org_type.CashModeCode, accounts.OrganisationStatusCode,
			COALESCE(accounts.TaskCashCode, accounts.PaymentCashCode) CashCode
		FROM accounts 
			INNER JOIN Org.tbType AS org_type ON accounts.OrganisationTypeCode = org_type.OrganisationTypeCode
