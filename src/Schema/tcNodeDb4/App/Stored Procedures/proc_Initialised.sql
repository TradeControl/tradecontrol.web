
CREATE   PROCEDURE App.proc_Initialised
(@Setting bit)
  AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @Setting = 1
			AND (EXISTS (SELECT     Subject.tbSubject.SubjectCode
						FROM         Subject.tbSubject INNER JOIN
											  App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode)
			OR EXISTS (SELECT     Subject.tbAddress.AddressCode
						   FROM         Subject.tbSubject INNER JOIN
												 App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode INNER JOIN
												 Subject.tbAddress ON Subject.tbSubject.AddressCode = Subject.tbAddress.AddressCode)
			OR EXISTS (SELECT     TOP 1 UserId
							   FROM         Usr.tbUser))
			BEGIN
			UPDATE App.tbOptions Set IsInitialised = 1
			RETURN
			END
		ELSE
			BEGIN
			UPDATE App.tbOptions Set IsInitialised = 0
			RETURN 1
			END
 	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
