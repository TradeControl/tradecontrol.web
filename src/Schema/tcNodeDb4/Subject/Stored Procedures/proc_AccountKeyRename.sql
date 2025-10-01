CREATE   PROCEDURE Subject.proc_AccountKeyRename(@AccountCode nvarchar (10), @OldKeyName nvarchar(50), @NewKeyName nvarchar(50), @KeyNamespace nvarchar(1024) output)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY

		UPDATE Subject.tbAccountKey
		SET KeyName = @NewKeyName
		WHERE AccountCode = @AccountCode AND KeyName = @OldKeyName;

		WITH namespaced AS
		(
			SELECT AccountCode, HDPath, CAST(KeyName as nvarchar(1024)) KeyNamespace, HDPath.GetLevel() HDLevel
			FROM Subject.tbAccountKey
			WHERE AccountCode = @AccountCode AND KeyName = @NewKeyName

			UNION ALL

			SELECT parent.AccountCode, parent.HDPath, CAST(CONCAT(parent.KeyName, '.', namespaced.KeyNamespace) as nvarchar(1024)) KeyNamespace, parent.HDPath.GetLevel() HDLevel
			FROM Subject.tbAccountKey parent
				JOIN namespaced ON parent.AccountCode = namespaced.AccountCode AND parent.HDPath = namespaced.HDPath.GetAncestor(1)
		)
		SELECT @KeyNamespace = REPLACE(UPPER(KeyNamespace), ' ', '_') 
		FROM namespaced
		WHERE HDLevel = 0;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
