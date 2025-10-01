
CREATE   PROCEDURE App.proc_DocDespool
	(
	@DocTypeCode SMALLINT
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @DocTypeCode = 0
		--Quotations:
			UPDATE       Task.tbTask
			SET           Spooled = 0, Printed = 1
			FROM            Task.tbTask INNER JOIN
									 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Task.tbTask.TaskStatusCode = 0) AND ( Cash.tbCategory.CashModeCode = 1) AND ( Task.tbTask.Spooled <> 0)
		ELSE IF @DocTypeCode = 1
		--SalesOrder:
			UPDATE       Task.tbTask
			SET           Spooled = 0, Printed = 1
			FROM            Task.tbTask INNER JOIN
									 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Task.tbTask.TaskStatusCode > 0) AND ( Cash.tbCategory.CashModeCode = 1) AND ( Task.tbTask.Spooled <> 0)
		ELSE IF @DocTypeCode = 2
		--PurchaseEnquiry:
			UPDATE       Task.tbTask
			SET           Spooled = 0, Printed = 1
			FROM            Task.tbTask INNER JOIN
									 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Task.tbTask.TaskStatusCode = 0) AND ( Cash.tbCategory.CashModeCode = 0) AND ( Task.tbTask.Spooled <> 0)	
		ELSE IF @DocTypeCode = 3
		--PurchaseOrder:
			UPDATE       Task.tbTask
			SET           Spooled = 0, Printed = 1
			FROM            Task.tbTask INNER JOIN
									 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Task.tbTask.TaskStatusCode > 0) AND ( Cash.tbCategory.CashModeCode = 0) AND ( Task.tbTask.Spooled <> 0)
		ELSE IF @DocTypeCode = 4
		--SalesInvoice:
			UPDATE       Invoice.tbInvoice
			SET                Spooled = 0, Printed = 1
			WHERE        (InvoiceTypeCode = 0) AND (Spooled <> 0)
		ELSE IF @DocTypeCode = 5
		--CreditNote:
			UPDATE       Invoice.tbInvoice
			SET                Spooled = 0, Printed = 1
			WHERE        (InvoiceTypeCode = 1) AND (Spooled <> 0)
		ELSE IF @DocTypeCode = 6
		--DebitNote:
			UPDATE       Invoice.tbInvoice
			SET                Spooled = 0, Printed = 1
			WHERE        (InvoiceTypeCode = 3) AND (Spooled <> 0)
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
