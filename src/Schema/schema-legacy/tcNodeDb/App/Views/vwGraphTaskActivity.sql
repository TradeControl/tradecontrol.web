CREATE VIEW App.vwGraphTaskActivity
AS
SELECT        CONCAT(Task.tbStatus.TaskStatus, SPACE(1), Cash.tbMode.CashMode) AS Category, SUM(Task.tbTask.TotalCharge) AS SumOfTotalCharge
FROM            Task.tbTask INNER JOIN
                         Task.tbStatus ON Task.tbTask.TaskStatusCode = Task.tbStatus.TaskStatusCode INNER JOIN
                         Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
                         Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
                         Cash.tbMode ON Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode
WHERE        (Task.tbTask.TaskStatusCode < 3) AND (Task.tbTask.TaskStatusCode > 0)
GROUP BY CONCAT(Task.tbStatus.TaskStatus, SPACE(1), Cash.tbMode.CashMode);
