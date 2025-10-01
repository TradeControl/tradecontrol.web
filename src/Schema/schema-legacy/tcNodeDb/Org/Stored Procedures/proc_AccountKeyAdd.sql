CREATE   PROCEDURE Org.proc_AccountKeyAdd (@CashAccountCode nvarchar (10), @ParentName nvarchar(50), @ChildName nvarchar(50), @ChildHDPath nvarchar(50) output)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY

		DECLARE @ParentId hierarchyid = (SELECT HDPath FROM Org.tbAccountKey WHERE CashAccountCode = @CashAccountCode AND KeyName = @ParentName);
		DECLARE @ChildId hierarchyId = (SELECT MAX(HDPath) FROM Org.tbAccountKey WHERE HDPath.GetAncestor(1) = @ParentId);

		IF (App.fnParsePrimaryKey(@ChildName) <> 0 AND CHARINDEX('.', @ChildName) = 0)
			BEGIN
				SET @ChildId = @ParentId.GetDescendant(@ChildId, NULL);

				INSERT INTO Org.tbAccountKey (CashAccountCode, HDPath, KeyName)
				SELECT @CashAccountCode CashAccountCode, 
					@ChildId HDPath, 
					@ChildName KeyName;

				SET @ChildHDPath = REPLACE(@ChildId.ToString(), '/', '''/'); 
				SET @ChildHDPath = RIGHT(@ChildHDPath, LEN(@ChildHDPath) - 1);
				SET @ChildHDPath = ( SELECT CONCAT('44/', CoinTypeCode, '/0', @ChildHDPath) FROM Org.tbAccount WHERE CashAccountCode = @CashAccountCode)
				
			END
		ELSE
			BEGIN
				DECLARE @Msg nvarchar(MAX) = (SELECT TOP (1) [Message] FROM App.tbText WHERE TextId = 2004);
				THROW 50000, @Msg, 1;				
			END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
