CREATE VIEW Invoice.vwCandidateSales
AS
	SELECT TOP 100 PERCENT TaskCode, AccountCode, ContactName, ActivityCode, ActionOn, ActionedOn, TaskTitle, Quantity, UnitCharge, TotalCharge, TaskNotes, CashDescription, ActionName, OwnerName, TaskStatus, InsertedBy, 
							 InsertedOn, UpdatedBy, UpdatedOn, TaskStatusCode
	FROM            Task.vwTasks
	WHERE        (TaskStatusCode = 1 OR
							 TaskStatusCode = 2) AND (CashModeCode = 1) AND (CashCode IS NOT NULL)
	ORDER BY ActionOn;
