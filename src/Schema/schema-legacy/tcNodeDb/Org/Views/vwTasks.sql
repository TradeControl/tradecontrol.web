CREATE   VIEW Org.vwTasks
AS
	SELECT        Task.vwTasks.AccountCode, Task.vwTasks.TaskCode, Task.vwTasks.UserId, Task.vwTasks.ContactName, Task.vwTasks.ActivityCode, Task.vwTasks.TaskTitle, Task.vwTasks.TaskStatusCode, Task.vwTasks.ActionById, 
							 Task.vwTasks.ActionOn, Task.vwTasks.ActionedOn, Task.vwTasks.PaymentOn, Task.vwTasks.SecondReference, Task.vwTasks.TaskNotes, Task.vwTasks.TaxCode, Task.vwTasks.Quantity, Task.vwTasks.UnitCharge, 
							 Task.vwTasks.TotalCharge, Task.vwTasks.AddressCodeFrom, Task.vwTasks.AddressCodeTo, Task.vwTasks.Printed, Task.vwTasks.Spooled, Task.vwTasks.InsertedBy, Task.vwTasks.InsertedOn, Task.vwTasks.UpdatedBy, 
							 Task.vwTasks.UpdatedOn, Task.vwTasks.Period, Task.vwTasks.BucketId, Task.vwTasks.TaskStatus, Task.vwTasks.CashCode, Task.vwTasks.CashDescription, Task.vwTasks.OwnerName, Task.vwTasks.ActionName, 
							 Task.vwTasks.AccountName, Task.vwTasks.OrganisationStatus, Task.vwTasks.OrganisationType, Task.vwTasks.CashModeCode, Cash.tbMode.CashMode
	FROM            Task.vwTasks INNER JOIN
							 Cash.tbMode ON Task.vwTasks.CashModeCode = Cash.tbMode.CashModeCode
	WHERE        (Task.vwTasks.CashCode IS NOT NULL)

