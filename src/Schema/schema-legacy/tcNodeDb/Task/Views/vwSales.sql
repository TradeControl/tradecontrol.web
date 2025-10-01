CREATE VIEW Task.vwSales
AS
	SELECT        Task.vwTasks.TaskCode, Task.vwTasks.ActivityCode, Task.vwTasks.TaskStatusCode, Task.vwTasks.ActionOn, Task.vwTasks.ActionById, Task.vwTasks.TaskTitle, Task.vwTasks.Period, Task.vwTasks.BucketId, 
							 Task.vwTasks.AccountCode, Task.vwTasks.ContactName, Task.vwTasks.TaskStatus, Task.vwTasks.TaskNotes, Task.vwTasks.ActionedOn, Task.vwTasks.OwnerName, Task.vwTasks.CashCode, 
							 Task.vwTasks.CashDescription, Task.vwTasks.Quantity, Activity.tbActivity.UnitOfMeasure, Task.vwTasks.UnitCharge, Task.vwTasks.TotalCharge, Org_tbAddress_1.Address AS FromAddress, 
							 Org.tbAddress.Address AS ToAddress, Task.vwTasks.Printed, Task.vwTasks.InsertedBy, Task.vwTasks.InsertedOn, Task.vwTasks.UpdatedBy, Task.vwTasks.UpdatedOn, Task.vwTasks.AccountName, 
							 Task.vwTasks.ActionName, Task.vwTasks.SecondReference
	FROM            Task.vwTasks LEFT OUTER JOIN
							 Org.tbAddress AS Org_tbAddress_1 ON Task.vwTasks.AddressCodeFrom = Org_tbAddress_1.AddressCode LEFT OUTER JOIN
							 Org.tbAddress ON Task.vwTasks.AddressCodeTo = Org.tbAddress.AddressCode INNER JOIN
							 Activity.tbActivity ON Task.vwTasks.ActivityCode = Activity.tbActivity.ActivityCode
	WHERE        (Task.vwTasks.CashCode IS NOT NULL) AND (Task.vwTasks.CashModeCode = 1);
