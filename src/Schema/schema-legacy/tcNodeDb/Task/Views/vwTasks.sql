CREATE VIEW Task.vwTasks
AS
	SELECT        Task.tbTask.TaskCode, Task.tbTask.UserId, Task.tbTask.AccountCode, Task.tbTask.ContactName, Task.tbTask.ActivityCode, Task.tbTask.TaskTitle, Task.tbTask.TaskStatusCode, Task.tbTask.ActionById, 
							 Task.tbTask.ActionOn, Task.tbTask.ActionedOn, Task.tbTask.PaymentOn, Task.tbTask.SecondReference, Task.tbTask.TaskNotes, Task.tbTask.TaxCode, Task.tbTask.Quantity, Task.tbTask.UnitCharge, 
							 Task.tbTask.TotalCharge, Task.tbTask.AddressCodeFrom, Task.tbTask.AddressCodeTo, Task.tbTask.Printed, Task.tbTask.Spooled, Task.tbTask.InsertedBy, Task.tbTask.InsertedOn, Task.tbTask.UpdatedBy, 
							 Task.tbTask.UpdatedOn, Task.vwBucket.Period, Task.vwBucket.BucketId, TaskStatus.TaskStatus, Task.tbTask.CashCode, Cash.tbCode.CashDescription, tbUser_1.UserName AS OwnerName, 
							 Usr.tbUser.UserName AS ActionName, Org.tbOrg.AccountName, OrgStatus.OrganisationStatus, Org.tbType.OrganisationType, CASE WHEN Cash.tbCategory.CategoryCode IS NULL 
							 THEN Org.tbType.CashModeCode ELSE Cash.tbCategory.CashModeCode END AS CashModeCode, Task.tbTask.RowVer
	FROM            Usr.tbUser INNER JOIN
							 Task.tbStatus AS TaskStatus INNER JOIN
							 Org.tbType INNER JOIN
							 Org.tbOrg ON Org.tbType.OrganisationTypeCode = Org.tbOrg.OrganisationTypeCode INNER JOIN
							 Org.tbStatus AS OrgStatus ON Org.tbOrg.OrganisationStatusCode = OrgStatus.OrganisationStatusCode INNER JOIN
							 Task.tbTask ON Org.tbOrg.AccountCode = Task.tbTask.AccountCode ON TaskStatus.TaskStatusCode = Task.tbTask.TaskStatusCode ON Usr.tbUser.UserId = Task.tbTask.ActionById INNER JOIN
							 Usr.tbUser AS tbUser_1 ON Task.tbTask.UserId = tbUser_1.UserId INNER JOIN
							 Task.vwBucket ON Task.tbTask.TaskCode = Task.vwBucket.TaskCode LEFT OUTER JOIN
							 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
