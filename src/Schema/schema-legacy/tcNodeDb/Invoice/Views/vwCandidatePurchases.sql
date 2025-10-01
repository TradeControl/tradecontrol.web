CREATE VIEW Invoice.vwCandidatePurchases
AS
	SELECT TOP 100 PERCENT  TaskCode, AccountCode, ContactName, ActivityCode, ActionOn, ActionedOn, Quantity, UnitCharge, TotalCharge, TaskTitle, TaskNotes, CashDescription, ActionName, OwnerName, TaskStatus, InsertedBy, 
							 InsertedOn, UpdatedBy, UpdatedOn, TaskStatusCode
	FROM            Task.vwTasks
	WHERE        (TaskStatusCode = 1 OR
							 TaskStatusCode = 2) AND (CashModeCode = 0) AND (CashCode IS NOT NULL)
	ORDER BY ActionOn;
