
CREATE   PROCEDURE Task.proc_NextCode
	(
		@ActivityCode nvarchar(50),
		@TaskCode nvarchar(20) OUTPUT
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@UserId nvarchar(10)
			, @NextTaskNumber int

		SELECT   @UserId = Usr.tbUser.UserId, @NextTaskNumber = Usr.tbUser.NextTaskNumber
		FROM         Usr.vwCredentials INNER JOIN
							Usr.tbUser ON Usr.vwCredentials.UserId = Usr.tbUser.UserId


		IF EXISTS(SELECT     App.tbRegister.NextNumber
				  FROM         Activity.tbActivity INNER JOIN
										App.tbRegister ON Activity.tbActivity.RegisterName = App.tbRegister.RegisterName
				  WHERE     ( Activity.tbActivity.ActivityCode = @ActivityCode))
			BEGIN
			DECLARE @RegisterName nvarchar(50)
			SELECT @RegisterName = App.tbRegister.RegisterName, @NextTaskNumber = App.tbRegister.NextNumber
			FROM         Activity.tbActivity INNER JOIN
										App.tbRegister ON Activity.tbActivity.RegisterName = App.tbRegister.RegisterName
			WHERE     ( Activity.tbActivity.ActivityCode = @ActivityCode)
			          
			UPDATE    App.tbRegister
			SET              NextNumber = NextNumber + 1
			WHERE     (RegisterName = @RegisterName)	
			END
		ELSE
			BEGIN	                      		
			UPDATE Usr.tbUser
			Set NextTaskNumber = NextTaskNumber + 1
			WHERE UserId = @UserId
			END
		                      
		SET @TaskCode = CONCAT(@UserId, '_', FORMAT(@NextTaskNumber, '0000'))
			                      
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
