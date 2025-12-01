CREATE VIEW Cash.vwPaymentsListing
AS
	SELECT Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName, Subject.tbType.SubjectType, Subject.tbStatus.SubjectStatus, Cash.tbPayment.PaymentCode, Usr.tbUser.UserName, 
							 App.tbTaxCode.TaxDescription AS PaymentTaxDescription, Subject.tbAccount.AccountName, Cash.tbCode.CashDescription, Cash.tbPayment.UserId, Cash.tbPayment.AccountCode, Cash.tbPayment.CashCode, 
							 Cash.tbPayment.TaxCode, CONCAT(YEAR(Cash.tbPayment.PaidOn), Format(MONTH(Cash.tbPayment.PaidOn), '00')) AS Period, Cash.tbPayment.PaidOn, Cash.tbPayment.PaidInValue, Cash.tbPayment.PaidOutValue, 
							 Cash.tbPayment.InsertedBy, Cash.tbPayment.InsertedOn, Cash.tbPayment.UpdatedBy, Cash.tbPayment.UpdatedOn, Cash.tbPayment.PaymentReference
	FROM            Cash.tbPayment INNER JOIN
							 Usr.tbUser ON Cash.tbPayment.UserId = Usr.tbUser.UserId INNER JOIN
							 Subject.tbAccount ON Cash.tbPayment.AccountCode = Subject.tbAccount.AccountCode INNER JOIN
							 Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Subject.tbSubject ON Cash.tbPayment.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Subject.tbType ON Subject.tbSubject.SubjectTypeCode = Subject.tbType.SubjectTypeCode INNER JOIN
							 Subject.tbStatus ON Subject.tbSubject.SubjectStatusCode = Subject.tbStatus.SubjectStatusCode
	WHERE        (Cash.tbPayment.PaymentStatusCode = 1);
