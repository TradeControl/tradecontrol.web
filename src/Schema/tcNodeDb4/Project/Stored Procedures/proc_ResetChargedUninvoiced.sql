
CREATE   PROCEDURE Project.proc_ResetChargedUninvoiced
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE       Project
		SET                ProjectStatusCode = 2
		FROM            Cash.tbCode INNER JOIN
								 Project.tbProject AS Project ON Cash.tbCode.CashCode = Project.CashCode LEFT OUTER JOIN
								 Invoice.tbProject AS InvoiceProject ON Project.ProjectCode = InvoiceProject.ProjectCode AND Project.ProjectCode = InvoiceProject.ProjectCode
		WHERE        (InvoiceProject.InvoiceNumber IS NULL) AND (Project.ProjectStatusCode = 3)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
