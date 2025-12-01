CREATE   PROCEDURE Subject.proc_AccountKeyDelete(@AccountCode nvarchar(10), @KeyName nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY	

		WITH root_level AS
		(
			SELECT AccountCode, CAST(NULL as hierarchyid) Ancestor, HDPath, HDPath.GetLevel() Lv
			FROM Subject.tbAccountKey 
			WHERE AccountCode = @AccountCode AND KeyName = @KeyName
		), candidates AS
		(
			SELECT ns.AccountCode, ns.HDPath.GetAncestor(1) Ancestor, ns.HDPath, ns.HDPath.GetLevel() Lv
			FROM Subject.tbAccountKey ns 
				JOIN root_level ON ns.AccountCode = root_level.AccountCode
			WHERE ns.HDPath.GetLevel() > root_level.Lv
		), selected AS
		(
			SELECT AccountCode, Ancestor, HDPath FROM root_level
		
			UNION ALL

			SELECT candidates.AccountCode, candidates.Ancestor, candidates.HDPath
			FROM candidates
				JOIN selected ON selected.HDPath = candidates.Ancestor
		)
		DELETE Subject.tbAccountKey
		FROM selected
			JOIN Subject.tbAccountKey ON Subject.tbAccountKey.AccountCode = selected.AccountCode AND Subject.tbAccountKey.HDPath = selected.HDPath;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
