CREATE   PROCEDURE Org.proc_WalletInitialise
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY
		WITH wallets AS
		(
			SELECT wallet.CashAccountCode
			FROM Org.vwWallets AS wallet 
				LEFT OUTER JOIN Org.tbAccountKey AS nspace ON wallet.CashAccountCode = nspace.CashAccountCode
			WHERE        (nspace.CashAccountCode IS NULL)
		), hdrootName AS
		(
			SELECT AccountName KeyName
			FROM Org.tbOrg orgs
				JOIN App.tbOptions opts ON opts.AccountCode = orgs.AccountCode
		)
		INSERT INTO Org.tbAccountKey (CashAccountCode, HDPath, KeyName)
		SELECT CashAccountCode, '/' HDPath, (SELECT KeyName FROM hdrootName) KeyName
		FROM wallets;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
