CREATE VIEW Cash.vwPaymentsUnposted
AS
	SELECT        PaymentCode, UserId, PaymentStatusCode, SubjectCode, AccountCode, CashCode, TaxCode, PaidOn, PaidInValue, PaidOutValue, PaymentReference, IsProfitAndLoss, InsertedBy, InsertedOn, 
							 UpdatedBy, UpdatedOn, RowVer
	FROM            Cash.tbPayment
	WHERE        (PaymentStatusCode = 0);
