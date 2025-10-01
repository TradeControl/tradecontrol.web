
CREATE   PROCEDURE Project.proc_Copy
	(
	@FromProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20) = null,
	@ToProjectCode nvarchar(20) = null output
	)
AS
	SET NOCOUNT, XACT_ABORT ON
	BEGIN TRY
		DECLARE 
			@ObjectCode nvarchar(50)
			, @Printed bit
			, @ChildProjectCode nvarchar(20)
			, @ProjectStatusCode smallint
			, @StepNumber smallint
			, @UserId nvarchar(10)
			, @SubjectCode nvarchar(10)

		SELECT @UserId = UserId FROM Usr.vwCredentials
	
		SELECT  
			@SubjectCode = Project.tbProject.SubjectCode,
			@ProjectStatusCode = Object.tbObject.ProjectStatusCode, 
			@ObjectCode = Project.tbProject.ObjectCode, 
			@Printed = CASE WHEN Object.tbObject.Printed = 0 THEN 1 ELSE 0 END
		FROM         Project.tbProject INNER JOIN
							  Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode
		WHERE     ( Project.tbProject.ProjectCode = @FromProjectCode)
	
		EXEC Project.proc_NextCode @ObjectCode, @ToProjectCode output

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION;

		INSERT INTO Project.tbProject
							  (ProjectCode, UserId, SubjectCode, ProjectTitle, ContactName, ObjectCode, ProjectStatusCode, ActionById, ActionOn, ActionedOn, ProjectNotes, Quantity, 
							  SecondReference, CashCode, TaxCode, UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, Printed)
		SELECT     @ToProjectCode AS ToProjectCode, @UserId AS Owner, SubjectCode, ProjectTitle, ContactName, ObjectCode, @ProjectStatusCode AS ProjectStatus, 
							  @UserId AS ActionUserId, CAST(CURRENT_TIMESTAMP AS date) AS ActionOn, 
							  CASE WHEN @ProjectStatusCode > 1 THEN CAST(CURRENT_TIMESTAMP AS date) ELSE NULL END AS ActionedOn, ProjectNotes, 
							  Quantity, SecondReference, CashCode, TaxCode, UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, @Printed AS Printed
		FROM         Project.tbProject AS Project_tb1
		WHERE     (ProjectCode = @FromProjectCode)
	
		INSERT INTO Project.tbAttribute
							  (ProjectCode, Attribute, PrintOrder, AttributeTypeCode, AttributeDescription)
		SELECT     @ToProjectCode AS ToProjectCode, Attribute, PrintOrder, AttributeTypeCode, AttributeDescription
		FROM         Project.tbAttribute 
		WHERE     (ProjectCode = @FromProjectCode)
	
		INSERT INTO Project.tbQuote
							  (ProjectCode, Quantity, TotalPrice, RunOnQuantity, RunOnPrice, RunBackQuantity, RunBackPrice)
		SELECT     @ToProjectCode AS ToProjectCode, Quantity, TotalPrice, RunOnQuantity, RunOnPrice, RunBackQuantity, RunBackPrice
		FROM         Project.tbQuote 
		WHERE     (ProjectCode = @FromProjectCode)
	
		INSERT INTO Project.tbOp
							  (ProjectCode, OperationNumber, OpStatusCode, UserId, SyncTypeCode, Operation, Note, StartOn, EndOn, Duration, OffsetDays)
		SELECT     @ToProjectCode AS ToProjectCode, OperationNumber, 0 AS OpStatusCode, UserId, SyncTypeCode, Operation, Note, 
			CAST(CURRENT_TIMESTAMP AS date) AS StartOn, CAST(CURRENT_TIMESTAMP AS date) AS EndOn, Duration, OffsetDays
		FROM         Project.tbOp 
		WHERE     (ProjectCode = @FromProjectCode)
	
		IF (ISNULL(@ParentProjectCode, '') = '')
			BEGIN
			IF EXISTS(SELECT     ParentProjectCode
					FROM         Project.tbFlow
					WHERE     (ChildProjectCode = @FromProjectCode))
				BEGIN
				SELECT @ParentProjectCode = ParentProjectCode
				FROM         Project.tbFlow
				WHERE     (ChildProjectCode = @FromProjectCode)

				SELECT @StepNumber = MAX(StepNumber)
				FROM         Project.tbFlow
				WHERE     (ParentProjectCode = @ParentProjectCode)
				GROUP BY ParentProjectCode
				
				SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10	
						
				INSERT INTO Project.tbFlow
				(ParentProjectCode, StepNumber, ChildProjectCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
				SELECT TOP 1 ParentProjectCode, @StepNumber AS Step, @ToProjectCode AS ChildProject, SyncTypeCode, UsedOnQuantity, OffsetDays
				FROM         Project.tbFlow
				WHERE     (ChildProjectCode = @FromProjectCode)
				END
			END
		ELSE
			BEGIN		
			INSERT INTO Project.tbFlow
			(ParentProjectCode, StepNumber, ChildProjectCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
			SELECT TOP 1 @ParentProjectCode As ParentProject, StepNumber, @ToProjectCode AS ChildProject, SyncTypeCode, UsedOnQuantity, OffsetDays
			FROM         Project.tbFlow 
			WHERE     (ChildProjectCode = @FromProjectCode)		
			END
	
		DECLARE curProject cursor local for			
			SELECT     ChildProjectCode
			FROM         Project.tbFlow
			WHERE     (ParentProjectCode = @FromProjectCode)
	
		OPEN curProject
	
		FETCH NEXT FROM curProject INTO @ChildProjectCode
		WHILE (@@FETCH_STATUS = 0)
			BEGIN
			EXEC Project.proc_Copy @ChildProjectCode, @ToProjectCode
			FETCH NEXT FROM curProject INTO @ChildProjectCode
			END
		
		CLOSE curProject
		DEALLOCATE curProject
		
		IF @@NESTLEVEL = 1
			BEGIN
			COMMIT TRANSACTION
			EXEC Project.proc_Schedule @ToProjectCode
			END

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
