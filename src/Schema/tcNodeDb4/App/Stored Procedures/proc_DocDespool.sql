
CREATE   PROCEDURE App.proc_DocDespool
	(
	@DocTypeCode SMALLINT
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @DocTypeCode = 0
		--Quotations:
			UPDATE       Project.tbProject
			SET           Spooled = 0, Printed = 1
			FROM            Project.tbProject INNER JOIN
									 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Project.tbProject.ProjectStatusCode = 0) AND ( Cash.tbCategory.CashPolarityCode = 1) AND ( Project.tbProject.Spooled <> 0)
		ELSE IF @DocTypeCode = 1
		--SalesOrder:
			UPDATE       Project.tbProject
			SET           Spooled = 0, Printed = 1
			FROM            Project.tbProject INNER JOIN
									 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Project.tbProject.ProjectStatusCode > 0) AND ( Cash.tbCategory.CashPolarityCode = 1) AND ( Project.tbProject.Spooled <> 0)
		ELSE IF @DocTypeCode = 2
		--PurchaseEnquiry:
			UPDATE       Project.tbProject
			SET           Spooled = 0, Printed = 1
			FROM            Project.tbProject INNER JOIN
									 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Project.tbProject.ProjectStatusCode = 0) AND ( Cash.tbCategory.CashPolarityCode = 0) AND ( Project.tbProject.Spooled <> 0)	
		ELSE IF @DocTypeCode = 3
		--PurchaseOrder:
			UPDATE       Project.tbProject
			SET           Spooled = 0, Printed = 1
			FROM            Project.tbProject INNER JOIN
									 Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE        ( Project.tbProject.ProjectStatusCode > 0) AND ( Cash.tbCategory.CashPolarityCode = 0) AND ( Project.tbProject.Spooled <> 0)
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
