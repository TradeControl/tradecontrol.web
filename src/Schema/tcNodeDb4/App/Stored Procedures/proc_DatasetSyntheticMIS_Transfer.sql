CREATE PROCEDURE App.proc_DatasetSyntheticMIS_Transfer
(
	@PaidOn date,
	@Amount decimal(18,5),
	@FromAccountCode nvarchar(10) = NULL,
	@ToAccountCode nvarchar(10) = NULL
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF @Amount IS NULL OR @Amount <= 0
		RETURN;

	DECLARE
		@HomeSubjectCode nvarchar(10) = (SELECT SubjectCode FROM App.vwHomeAccount),
		@DefaultFrom nvarchar(10) = (SELECT AccountCode FROM Cash.vwCurrentAccount),
		@DefaultTo nvarchar(10) = (SELECT AccountCode FROM Cash.vwReserveAccount),
		@UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials);

	SET @FromAccountCode = COALESCE(@FromAccountCode, @DefaultFrom);
	SET @ToAccountCode = COALESCE(@ToAccountCode, @DefaultTo);

	IF @HomeSubjectCode IS NULL OR @FromAccountCode IS NULL OR @ToAccountCode IS NULL
		THROW 51330, 'DatasetSyntheticMIS_Transfer: missing Home/From/To account lookup.', 1;

	DECLARE
		@PayOutCode nvarchar(20) = NULL,
		@PayInCode nvarchar(20) = NULL;

	EXEC Cash.proc_NextPaymentCode @PaymentCode = @PayOutCode OUTPUT;

	-- From account pays out (TC800)
	INSERT INTO Cash.tbPayment
	(
		PaymentCode,
		UserId,
		PaymentStatusCode,
		SubjectCode,
		AccountCode,
		CashCode,
		TaxCode,
		PaidOn,
		PaidInValue,
		PaidOutValue,
		PaymentReference
	)
	VALUES
	(
		@PayOutCode,
		@UserId,
		2,
		@HomeSubjectCode,
		@FromAccountCode,
		N'TC800',
		N'N/A',
		@PaidOn,
		0.00000,
		@Amount,
		N'DS TRANSFER'
	);

	EXEC Cash.proc_NextPaymentCode @PaymentCode = @PayInCode OUTPUT;

	-- To account receives (TC801)
	INSERT INTO Cash.tbPayment
	(
		PaymentCode,
		UserId,
		PaymentStatusCode,
		SubjectCode,
		AccountCode,
		CashCode,
		TaxCode,
		PaidOn,
		PaidInValue,
		PaidOutValue,
		PaymentReference
	)
	VALUES
	(
		@PayInCode,
		@UserId,
		2,
		@HomeSubjectCode,
		@ToAccountCode,
		N'TC801',
		N'N/A',
		@PaidOn,
		@Amount,
		0.00000,
		N'DS TRANSFER'
	);

	-- Post both legs
	EXEC Cash.proc_PayAccrual @PaymentCode = @PayOutCode;
	EXEC Cash.proc_PayAccrual @PaymentCode = @PayInCode;
GO
