
CREATE   VIEW Subject.vwCompanyLogo
AS
SELECT        TOP (1) Subject.tbSubject.Logo
FROM            Subject.tbSubject INNER JOIN
                         App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode;
