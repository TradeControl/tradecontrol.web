
CREATE   VIEW Project.vwActiveStatusCodes
AS
SELECT        ProjectStatusCode, ProjectStatus
FROM            Project.tbStatus
WHERE        (ProjectStatusCode < 3);
