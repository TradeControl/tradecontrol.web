CREATE VIEW Task.vwFlow
AS
	SELECT        Task.tbFlow.ParentTaskCode, Task.tbFlow.StepNumber, Task.tbTask.TaskCode, Task.tbTask.ActivityCode, Task.tbTask.TaskTitle, Task.tbTask.TaskNotes, Task.tbStatus.TaskStatus, Task.tbTask.ActionOn, 
							 Task.tbTask.Quantity, Task.tbTask.ActionedOn, Org.tbOrg.AccountCode, Usr.tbUser.UserName AS Owner, tbUser_1.UserName AS ActionBy, Org.tbOrg.AccountName, Task.tbTask.UnitCharge, 
							 Task.tbTask.TotalCharge, Task.tbTask.InsertedBy, Task.tbTask.InsertedOn, Task.tbTask.UpdatedBy, Task.tbTask.UpdatedOn, Task.tbTask.TaskStatusCode
	FROM            Usr.tbUser AS tbUser_1 INNER JOIN
							 Task.tbTask INNER JOIN
							 Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
							 Usr.tbUser ON Task.tbTask.UserId = Usr.tbUser.UserId INNER JOIN
							 Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode ON tbUser_1.UserId = Task.tbTask.ActionById INNER JOIN
							 Task.tbFlow ON Task.tbTask.TaskCode = Task.tbFlow.ChildTaskCode;
