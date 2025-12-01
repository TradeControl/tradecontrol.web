CREATE   PROCEDURE Subject.proc_WalletInitialise
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY
		WITH wallets AS
		(
			SELECT wallet.AccountCode
			FROM Subject.vwWallets AS wallet 
				LEFT OUTER JOIN Subject.tbAccountKey AS nspace ON wallet.AccountCode = nspace.AccountCode
			WHERE        (nspace.AccountCode IS NULL)
		), hdrootName AS
		(
			SELECT SubjectName KeyName
			FROM Subject.tbSubject Subjects
				JOIN App.tbOptions opts ON opts.SubjectCode = Subjects.SubjectCode
		)
		INSERT INTO Subject.tbAccountKey (AccountCode, HDPath, KeyName)
		SELECT AccountCode, '/' HDPath, (SELECT KeyName FROM hdrootName) KeyName
		FROM wallets;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
