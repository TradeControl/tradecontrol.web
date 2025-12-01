
CREATE   VIEW Project.vwTitles
AS
SELECT        ObjectCode, ProjectTitle
FROM            Project.tbProject
GROUP BY ProjectTitle, ObjectCode
HAVING        (ProjectTitle IS NOT NULL);
