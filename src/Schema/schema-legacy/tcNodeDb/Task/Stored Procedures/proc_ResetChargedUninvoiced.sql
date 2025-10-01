
CREATE   PROCEDURE Task.proc_ResetChargedUninvoiced
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE       Task
		SET                TaskStatusCode = 2
		FROM            Cash.tbCode INNER JOIN
								 Task.tbTask AS Task ON Cash.tbCode.CashCode = Task.CashCode LEFT OUTER JOIN
								 Invoice.tbTask AS InvoiceTask ON Task.TaskCode = InvoiceTask.TaskCode AND Task.TaskCode = InvoiceTask.TaskCode
		WHERE        (InvoiceTask.InvoiceNumber IS NULL) AND (Task.TaskStatusCode = 3)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
