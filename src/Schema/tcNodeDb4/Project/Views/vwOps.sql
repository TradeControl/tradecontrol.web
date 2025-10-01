CREATE VIEW Project.vwOps
AS
SELECT        Project.tbOp.ProjectCode, Project.tbProject.ObjectCode, Project.tbOp.OperationNumber, Project.vwOpBucket.Period, Project.vwOpBucket.BucketId, Project.tbOp.UserId, Project.tbOp.SyncTypeCode, Project.tbOp.OpStatusCode, 
                         Project.tbOp.Operation, Project.tbOp.Note, Project.tbOp.StartOn, Project.tbOp.EndOn, Project.tbOp.Duration, Project.tbOp.OffsetDays, Project.tbOp.InsertedBy, Project.tbOp.InsertedOn, Project.tbOp.UpdatedBy, Project.tbOp.UpdatedOn, 
                         Project.tbProject.ProjectTitle, Project.tbProject.ProjectStatusCode, Project.tbStatus.ProjectStatus, Project.tbProject.ActionOn, Project.tbProject.Quantity, Cash.tbCode.CashDescription, Project.tbProject.TotalCharge, Project.tbProject.SubjectCode, 
                         Subject.tbSubject.SubjectName, Project.tbOp.RowVer AS OpRowVer, Project.tbProject.RowVer AS ProjectRowVer
FROM            Project.tbOp INNER JOIN
                         Project.tbProject ON Project.tbOp.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
                         Project.tbStatus ON Project.tbProject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
                         Project.vwOpBucket ON Project.tbOp.ProjectCode = Project.vwOpBucket.ProjectCode AND Project.tbOp.OperationNumber = Project.vwOpBucket.OperationNumber LEFT OUTER JOIN
                         Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode

