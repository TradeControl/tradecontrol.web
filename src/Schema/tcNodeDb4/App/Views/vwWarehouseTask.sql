
CREATE   VIEW App.vwWarehouseProject
AS
SELECT TOP (100) PERCENT Project.tbDoc.ProjectCode, Project.tbDoc.DocumentName, Subject.tbSubject.SubjectName, Project.tbProject.ProjectTitle, Project.tbDoc.DocumentImage, Project.tbDoc.DocumentDescription, Project.tbDoc.InsertedBy, Project.tbDoc.InsertedOn, 
                         Project.tbDoc.UpdatedBy, Project.tbDoc.UpdatedOn, Project.tbDoc.RowVer
FROM            Subject.tbSubject INNER JOIN
                         Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode INNER JOIN
                         Project.tbDoc ON Project.tbProject.ProjectCode = Project.tbDoc.ProjectCode
ORDER BY Project.tbDoc.ProjectCode, Project.tbDoc.DocumentName;
