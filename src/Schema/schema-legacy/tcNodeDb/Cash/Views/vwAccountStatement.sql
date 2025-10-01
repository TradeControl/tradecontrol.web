CREATE VIEW Cash.vwAccountStatement
AS
	WITH entries AS
	(
		SELECT  payment.CashAccountCode, payment.CashCode, ROW_NUMBER() OVER (PARTITION BY payment.CashAccountCode ORDER BY PaidOn) AS EntryNumber, PaymentCode, PaidOn, 
			CASE WHEN PaidInValue > 0 THEN PaidInValue ELSE PaidOutValue * - 1 END AS Paid
		FROM         Cash.tbPayment payment INNER JOIN Org.tbAccount ON payment.CashAccountCode = Org.tbAccount.CashAccountCode
		WHERE     (PaymentStatusCode = 1) AND (AccountClosed = 0)	
		UNION
		SELECT        
			CashAccountCode, 
			COALESCE(CashCode, (SELECT TOP 1 CashCode FROM Cash.vwBankCashCodes WHERE CashModeCode = 2)) CashCode,
			0 AS EntryNumber, 
			(SELECT CAST(Message AS NVARCHAR(30)) FROM App.tbText WHERE TextId = 3005) AS PaymentCode, 
			DATEADD(HOUR, - 1, (SELECT MIN(PaidOn) FROM Cash.tbPayment WHERE CashAccountCode = cash_account.CashAccountCode)) AS PaidOn, OpeningBalance AS Paid
		FROM            Org.tbAccount cash_account 								 
		WHERE        (AccountClosed = 0) 
	), running_balance AS
	(
		SELECT CashAccountCode, CashCode, EntryNumber, PaymentCode, PaidOn, 
			SUM(Paid) OVER (PARTITION BY CashAccountCode ORDER BY EntryNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS PaidBalance
		FROM entries
	), payments AS
	(
		SELECT     Cash.tbPayment.PaymentCode, Cash.tbPayment.CashAccountCode, Usr.tbUser.UserName, Cash.tbPayment.AccountCode, 
							  Org.tbOrg.AccountName, Cash.tbPayment.CashCode, Cash.tbCode.CashDescription, App.tbTaxCode.TaxDescription, 
							  Cash.tbPayment.PaidInValue, Cash.tbPayment.PaidOutValue, 
							  Cash.tbPayment.PaymentReference, Cash.tbPayment.InsertedBy, Cash.tbPayment.InsertedOn, 
							  Cash.tbPayment.UpdatedBy, Cash.tbPayment.UpdatedOn, Cash.tbPayment.TaxCode
		FROM         Cash.tbPayment INNER JOIN
							  Usr.tbUser ON Cash.tbPayment.UserId = Usr.tbUser.UserId INNER JOIN
							  Org.tbOrg ON Cash.tbPayment.AccountCode = Org.tbOrg.AccountCode LEFT OUTER JOIN
							  App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							  Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode
	)
		SELECT running_balance.CashAccountCode, 
			COALESCE((SELECT TOP 1 StartOn FROM App.tbYearPeriod WHERE (StartOn <= running_balance.PaidOn) ORDER BY StartOn DESC), 
				(SELECT MIN(StartOn) FROM App.tbYearPeriod) ) AS StartOn, 
			running_balance.EntryNumber, running_balance.PaymentCode, running_balance.PaidOn, 
			payments.AccountName, payments.PaymentReference, COALESCE(payments.PaidInValue, 0) PaidInValue, 
			COALESCE(payments.PaidOutValue, 0) PaidOutValue, CAST(running_balance.PaidBalance as decimal(18,5)) PaidBalance, 
			payments.CashCode, payments.CashDescription, payments.TaxDescription, payments.UserName, 
			payments.AccountCode, payments.TaxCode
		FROM   running_balance LEFT OUTER JOIN
								payments ON running_balance.PaymentCode = payments.PaymentCode

