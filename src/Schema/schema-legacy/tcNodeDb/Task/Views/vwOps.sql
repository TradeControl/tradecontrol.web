CREATE VIEW Task.vwOps
AS
SELECT        Task.tbOp.TaskCode, Task.tbTask.ActivityCode, Task.tbOp.OperationNumber, Task.vwOpBucket.Period, Task.vwOpBucket.BucketId, Task.tbOp.UserId, Task.tbOp.SyncTypeCode, Task.tbOp.OpStatusCode, 
                         Task.tbOp.Operation, Task.tbOp.Note, Task.tbOp.StartOn, Task.tbOp.EndOn, Task.tbOp.Duration, Task.tbOp.OffsetDays, Task.tbOp.InsertedBy, Task.tbOp.InsertedOn, Task.tbOp.UpdatedBy, Task.tbOp.UpdatedOn, 
                         Task.tbTask.TaskTitle, Task.tbTask.TaskStatusCode, Task.tbStatus.TaskStatus, Task.tbTask.ActionOn, Task.tbTask.Quantity, Cash.tbCode.CashDescription, Task.tbTask.TotalCharge, Task.tbTask.AccountCode, 
                         Org.tbOrg.AccountName, Task.tbOp.RowVer AS OpRowVer, Task.tbTask.RowVer AS TaskRowVer
FROM            Task.tbOp INNER JOIN
                         Task.tbTask ON Task.tbOp.TaskCode = Task.tbTask.TaskCode INNER JOIN
                         Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode INNER JOIN
                         Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
                         Task.vwOpBucket ON Task.tbOp.TaskCode = Task.vwOpBucket.TaskCode AND Task.tbOp.OperationNumber = Task.vwOpBucket.OperationNumber LEFT OUTER JOIN
                         Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode

