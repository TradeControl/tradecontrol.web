
CREATE   VIEW Task.vwActiveStatusCodes
AS
SELECT        TaskStatusCode, TaskStatus
FROM            Task.tbStatus
WHERE        (TaskStatusCode < 3);
