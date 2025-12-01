
CREATE   VIEW App.vwHomeAccount
AS
	SELECT     Subject.tbSubject.SubjectCode, Subject.tbSubject.SubjectName
	FROM            App.tbOptions INNER JOIN
							 Subject.tbSubject ON App.tbOptions.SubjectCode = Subject.tbSubject.SubjectCode
