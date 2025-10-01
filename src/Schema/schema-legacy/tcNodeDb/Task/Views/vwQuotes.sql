CREATE   VIEW Task.vwQuotes
AS
	SELECT        Task.tbTask.UserId, Cash.tbCategory.CashModeCode, Cash.tbMode.CashMode, Task.tbTask.ActionOn, Task.tbTask.TaskCode, Task.tbTask.AccountCode, Task.tbTask.ContactName, Task.tbTask.ActivityCode, 
							 Task.tbTask.TaskTitle, Task.tbTask.SecondReference, Task.tbTask.TaxCode, Task.tbTask.Quantity, Task.tbTask.UnitCharge, Task.tbTask.TotalCharge, Task.vwBucket.Period, Task.vwBucket.BucketId, Task.tbTask.CashCode, 
							 Cash.tbCode.CashDescription, tbUser_1.UserName AS OwnerName, Org.tbOrg.AccountName, Task.tbTask.RowVer
	FROM            Org.tbOrg INNER JOIN
							 Task.tbTask ON Org.tbOrg.AccountCode = Task.tbTask.AccountCode INNER JOIN
							 Usr.tbUser AS tbUser_1 ON Task.tbTask.UserId = tbUser_1.UserId INNER JOIN
							 Task.vwBucket ON Task.tbTask.TaskCode = Task.vwBucket.TaskCode INNER JOIN
							 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbMode ON Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode
	WHERE        (Task.tbTask.TaskStatusCode = 0);
