CREATE VIEW Task.vwOrgActivity
AS
	SELECT AccountCode, TaskStatusCode, ActionOn, TaskTitle, ActivityCode, ActionById, TaskCode, Period, BucketId, ContactName, TaskStatus, TaskNotes, ActionedOn, OwnerName, CashCode, CashDescription, Quantity, 
							 UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, Printed, InsertedBy, InsertedOn, UpdatedBy, UpdatedOn, AccountName, ActionName
	FROM            Task.vwTasks
	WHERE        (TaskStatusCode < 2);
