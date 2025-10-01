
CREATE   PROCEDURE Org.proc_AddAddress 
	(
	@AccountCode nvarchar(10),
	@Address ntext
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @AddressCode nvarchar(15)
	
		EXECUTE Org.proc_NextAddressCode @AccountCode, @AddressCode OUTPUT
	
		INSERT INTO Org.tbAddress
							  (AddressCode, AccountCode, Address)
		VALUES     (@AddressCode, @AccountCode, @Address)
	
		IF NOT EXISTS (SELECT * FROM Org.tbOrg org JOIN Org.tbAddress org_addr ON org.AddressCode = org_addr.AddressCode WHERE org.AccountCode = @AccountCode)
		BEGIN
			UPDATE Org.tbOrg
			SET AddressCode = @AddressCode
			WHERE Org.tbOrg.AccountCode = @AccountCode
		END

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
