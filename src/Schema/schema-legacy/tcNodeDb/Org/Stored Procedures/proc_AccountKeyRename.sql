CREATE   PROCEDURE Org.proc_AccountKeyRename(@CashAccountCode nvarchar (10), @OldKeyName nvarchar(50), @NewKeyName nvarchar(50), @KeyNamespace nvarchar(1024) output)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY

		UPDATE Org.tbAccountKey
		SET KeyName = @NewKeyName
		WHERE CashAccountCode = @CashAccountCode AND KeyName = @OldKeyName;

		WITH namespaced AS
		(
			SELECT CashAccountCode, HDPath, CAST(KeyName as nvarchar(1024)) KeyNamespace, HDPath.GetLevel() HDLevel
			FROM Org.tbAccountKey
			WHERE CashAccountCode = @CashAccountCode AND KeyName = @NewKeyName

			UNION ALL

			SELECT parent.CashAccountCode, parent.HDPath, CAST(CONCAT(parent.KeyName, '.', namespaced.KeyNamespace) as nvarchar(1024)) KeyNamespace, parent.HDPath.GetLevel() HDLevel
			FROM Org.tbAccountKey parent
				JOIN namespaced ON parent.CashAccountCode = namespaced.CashAccountCode AND parent.HDPath = namespaced.HDPath.GetAncestor(1)
		)
		SELECT @KeyNamespace = REPLACE(UPPER(KeyNamespace), ' ', '_') 
		FROM namespaced
		WHERE HDLevel = 0;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
