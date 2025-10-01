CREATE VIEW Task.vwEdit
AS
	SELECT        Task.tbTask.TaskCode, Task.tbTask.UserId, Task.tbTask.AccountCode, Task.tbTask.TaskTitle, Task.tbTask.ContactName, Task.tbTask.ActivityCode, Task.tbTask.TaskStatusCode, Task.tbTask.ActionById, 
							 Task.tbTask.ActionOn, Task.tbTask.ActionedOn, Task.tbTask.TaskNotes, Task.tbTask.Quantity, Task.tbTask.CashCode, Task.tbTask.TaxCode, Task.tbTask.UnitCharge, Task.tbTask.TotalCharge, 
							 Task.tbTask.AddressCodeFrom, Task.tbTask.AddressCodeTo, Task.tbTask.Printed, Task.tbTask.InsertedBy, Task.tbTask.InsertedOn, Task.tbTask.UpdatedBy, Task.tbTask.UpdatedOn, Task.tbTask.PaymentOn, 
							 Task.tbTask.SecondReference, Task.tbTask.Spooled, Activity.tbActivity.UnitOfMeasure, Task.tbStatus.TaskStatus
	FROM            Task.tbTask INNER JOIN
							 Activity.tbActivity ON Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode INNER JOIN
							 Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode;
