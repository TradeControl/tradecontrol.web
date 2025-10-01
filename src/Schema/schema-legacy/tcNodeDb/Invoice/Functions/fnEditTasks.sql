CREATE   FUNCTION Invoice.fnEditTasks (@InvoiceNumber nvarchar(20), @AccountCode nvarchar(10))
RETURNS TABLE
AS
	RETURN 
	(		
		WITH InvoiceEditTasks AS 
		(	SELECT        TaskCode
			FROM            Invoice.tbTask
			WHERE        (InvoiceNumber = @InvoiceNumber)
		)
		SELECT TOP (100) PERCENT Task.tbTask.TaskCode, Task.tbTask.ActivityCode, Task.tbStatus.TaskStatus, Usr.tbUser.UserName, Task.tbTask.ActionOn, Task.tbTask.ActionedOn, Task.tbTask.TaskTitle
		FROM            Usr.tbUser INNER JOIN
								Task.tbTask INNER JOIN
								Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode ON Usr.tbUser.UserId = Task.tbTask.ActionById LEFT OUTER JOIN
								InvoiceEditTasks ON Task.tbTask.TaskCode = InvoiceEditTasks.TaskCode
		WHERE        (Task.tbTask.AccountCode = @AccountCode) AND (Task.tbTask.TaskStatusCode = 1 OR
								Task.tbTask.TaskStatusCode = 2) AND (Task.tbTask.CashCode IS NOT NULL) AND (InvoiceEditTasks.TaskCode IS NULL)
		ORDER BY Task.tbTask.ActionOn DESC
	);
