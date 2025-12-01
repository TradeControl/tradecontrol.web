
CREATE   PROCEDURE Project.proc_NextCode
	(
		@ObjectCode nvarchar(50),
		@ProjectCode nvarchar(20) OUTPUT
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@UserId nvarchar(10)
			, @NextProjectNumber int

		SELECT   @UserId = Usr.tbUser.UserId, @NextProjectNumber = Usr.tbUser.NextProjectNumber
		FROM         Usr.vwCredentials INNER JOIN
							Usr.tbUser ON Usr.vwCredentials.UserId = Usr.tbUser.UserId


		IF EXISTS(SELECT     App.tbRegister.NextNumber
				  FROM         Object.tbObject INNER JOIN
										App.tbRegister ON Object.tbObject.RegisterName = App.tbRegister.RegisterName
				  WHERE     ( Object.tbObject.ObjectCode = @ObjectCode))
			BEGIN
			DECLARE @RegisterName nvarchar(50)
			SELECT @RegisterName = App.tbRegister.RegisterName, @NextProjectNumber = App.tbRegister.NextNumber
			FROM         Object.tbObject INNER JOIN
										App.tbRegister ON Object.tbObject.RegisterName = App.tbRegister.RegisterName
			WHERE     ( Object.tbObject.ObjectCode = @ObjectCode)
			          
			UPDATE    App.tbRegister
			SET              NextNumber = NextNumber + 1
			WHERE     (RegisterName = @RegisterName)	
			END
		ELSE
			BEGIN	                      		
			UPDATE Usr.tbUser
			Set NextProjectNumber = NextProjectNumber + 1
			WHERE UserId = @UserId
			END
		                      
		SET @ProjectCode = CONCAT(@UserId, '_', FORMAT(@NextProjectNumber, '0000'))
			                      
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
