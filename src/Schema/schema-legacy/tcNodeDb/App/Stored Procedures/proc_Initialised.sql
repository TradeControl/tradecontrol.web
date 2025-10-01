
CREATE   PROCEDURE App.proc_Initialised
(@Setting bit)
  AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @Setting = 1
			AND (EXISTS (SELECT     Org.tbOrg.AccountCode
						FROM         Org.tbOrg INNER JOIN
											  App.tbOptions ON Org.tbOrg.AccountCode = App.tbOptions.AccountCode)
			OR EXISTS (SELECT     Org.tbAddress.AddressCode
						   FROM         Org.tbOrg INNER JOIN
												 App.tbOptions ON Org.tbOrg.AccountCode = App.tbOptions.AccountCode INNER JOIN
												 Org.tbAddress ON Org.tbOrg.AddressCode = Org.tbAddress.AddressCode)
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
