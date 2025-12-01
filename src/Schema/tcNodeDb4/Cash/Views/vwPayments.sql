CREATE   VIEW Cash.vwPayments
AS
	SELECT        Cash.tbPayment.PaymentCode, Cash.tbPayment.PaymentStatusCode, Cash.tbPayment.UserId, Usr.tbUser.UserName, Subject.tbSubject.SubjectName, Cash.tbPayment.SubjectCode, Cash.tbPayment.AccountCode, Subject.tbAccount.AccountName, 
							 Cash.tbPayment.CashCode, Cash.tbCode.CashDescription, Cash.tbPayment.TaxCode, App.tbTaxCode.TaxDescription, Cash.tbPayment.PaidOn, Cash.tbPayment.PaidInValue, Cash.tbPayment.PaidOutValue, 
							 Cash.tbPayment.PaymentReference, Cash.tbPayment.IsProfitAndLoss, Cash.tbPayment.InsertedBy, Cash.tbPayment.InsertedOn, Cash.tbPayment.UpdatedBy, Cash.tbPayment.UpdatedOn
	FROM            Cash.tbPayment INNER JOIN
							 Usr.tbUser ON Cash.tbPayment.UserId = Usr.tbUser.UserId INNER JOIN
							 Subject.tbSubject ON Cash.tbPayment.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Subject.tbAccount ON Cash.tbPayment.AccountCode = Subject.tbAccount.AccountCode LEFT OUTER JOIN
							 App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							 Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode
