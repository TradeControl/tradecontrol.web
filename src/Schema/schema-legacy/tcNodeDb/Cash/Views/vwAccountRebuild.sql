CREATE VIEW Cash.vwAccountRebuild
AS
	SELECT     Cash.tbPayment.CashAccountCode, Org.tbAccount.OpeningBalance, 
						  Org.tbAccount.OpeningBalance + SUM(Cash.tbPayment.PaidInValue - Cash.tbPayment.PaidOutValue) AS CurrentBalance
	FROM         Cash.tbPayment INNER JOIN
						  Org.tbAccount ON Cash.tbPayment.CashAccountCode = Org.tbAccount.CashAccountCode
	WHERE     (Cash.tbPayment.PaymentStatusCode = 1) 
	GROUP BY Cash.tbPayment.CashAccountCode, Org.tbAccount.OpeningBalance
