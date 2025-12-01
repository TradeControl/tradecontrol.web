
CREATE   VIEW App.vwWarehouseSubject
AS
SELECT TOP (100) PERCENT Subject.tbSubject.SubjectCode, Subject.tbDoc.DocumentName, Subject.tbSubject.SubjectName, Subject.tbDoc.DocumentImage, Subject.tbDoc.DocumentDescription, Subject.tbDoc.InsertedBy, Subject.tbDoc.InsertedOn, Subject.tbDoc.UpdatedBy, 
                         Subject.tbDoc.UpdatedOn, Subject.tbDoc.RowVer
FROM            Subject.tbSubject INNER JOIN
                         Subject.tbDoc ON Subject.tbSubject.SubjectCode = Subject.tbDoc.SubjectCode
ORDER BY Subject.tbDoc.SubjectCode, Subject.tbDoc.DocumentName;
