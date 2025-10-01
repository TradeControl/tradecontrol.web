
CREATE   VIEW App.vwWarehouseTask
AS
SELECT TOP (100) PERCENT Task.tbDoc.TaskCode, Task.tbDoc.DocumentName, Org.tbOrg.AccountName, Task.tbTask.TaskTitle, Task.tbDoc.DocumentImage, Task.tbDoc.DocumentDescription, Task.tbDoc.InsertedBy, Task.tbDoc.InsertedOn, 
                         Task.tbDoc.UpdatedBy, Task.tbDoc.UpdatedOn, Task.tbDoc.RowVer
FROM            Org.tbOrg INNER JOIN
                         Task.tbTask ON Org.tbOrg.AccountCode = Task.tbTask.AccountCode INNER JOIN
                         Task.tbDoc ON Task.tbTask.TaskCode = Task.tbDoc.TaskCode
ORDER BY Task.tbDoc.TaskCode, Task.tbDoc.DocumentName;
