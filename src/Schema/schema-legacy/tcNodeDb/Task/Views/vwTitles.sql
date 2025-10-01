
CREATE   VIEW Task.vwTitles
AS
SELECT        ActivityCode, TaskTitle
FROM            Task.tbTask
GROUP BY TaskTitle, ActivityCode
HAVING        (TaskTitle IS NOT NULL);
