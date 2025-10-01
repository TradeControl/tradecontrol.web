
CREATE   PROCEDURE App.proc_CompanyName
	(
	@SubjectName nvarchar(255) = null output
	)
  AS
	SELECT TOP 1 @SubjectName = Subject.tbSubject.SubjectName
	FROM         Subject.tbSubject INNER JOIN
	                      App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode
	 
