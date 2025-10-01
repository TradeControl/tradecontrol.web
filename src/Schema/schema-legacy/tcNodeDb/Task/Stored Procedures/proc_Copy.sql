
CREATE   PROCEDURE Task.proc_Copy
	(
	@FromTaskCode nvarchar(20),
	@ParentTaskCode nvarchar(20) = null,
	@ToTaskCode nvarchar(20) = null output
	)
AS
	SET NOCOUNT, XACT_ABORT ON
	BEGIN TRY
		DECLARE 
			@ActivityCode nvarchar(50)
			, @Printed bit
			, @ChildTaskCode nvarchar(20)
			, @TaskStatusCode smallint
			, @StepNumber smallint
			, @UserId nvarchar(10)
			, @AccountCode nvarchar(10)

		SELECT @UserId = UserId FROM Usr.vwCredentials
	
		SELECT  
			@AccountCode = Task.tbTask.AccountCode,
			@TaskStatusCode = Activity.tbActivity.TaskStatusCode, 
			@ActivityCode = Task.tbTask.ActivityCode, 
			@Printed = CASE WHEN Activity.tbActivity.Printed = 0 THEN 1 ELSE 0 END
		FROM         Task.tbTask INNER JOIN
							  Activity.tbActivity ON Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode
		WHERE     ( Task.tbTask.TaskCode = @FromTaskCode)
	
		EXEC Task.proc_NextCode @ActivityCode, @ToTaskCode output

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION;

		INSERT INTO Task.tbTask
							  (TaskCode, UserId, AccountCode, TaskTitle, ContactName, ActivityCode, TaskStatusCode, ActionById, ActionOn, ActionedOn, TaskNotes, Quantity, 
							  SecondReference, CashCode, TaxCode, UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, Printed)
		SELECT     @ToTaskCode AS ToTaskCode, @UserId AS Owner, AccountCode, TaskTitle, ContactName, ActivityCode, @TaskStatusCode AS TaskStatus, 
							  @UserId AS ActionUserId, CAST(CURRENT_TIMESTAMP AS date) AS ActionOn, 
							  CASE WHEN @TaskStatusCode > 1 THEN CAST(CURRENT_TIMESTAMP AS date) ELSE NULL END AS ActionedOn, TaskNotes, 
							  Quantity, SecondReference, CashCode, TaxCode, UnitCharge, TotalCharge, AddressCodeFrom, AddressCodeTo, @Printed AS Printed
		FROM         Task.tbTask AS Task_tb1
		WHERE     (TaskCode = @FromTaskCode)
	
		INSERT INTO Task.tbAttribute
							  (TaskCode, Attribute, PrintOrder, AttributeTypeCode, AttributeDescription)
		SELECT     @ToTaskCode AS ToTaskCode, Attribute, PrintOrder, AttributeTypeCode, AttributeDescription
		FROM         Task.tbAttribute 
		WHERE     (TaskCode = @FromTaskCode)
	
		INSERT INTO Task.tbQuote
							  (TaskCode, Quantity, TotalPrice, RunOnQuantity, RunOnPrice, RunBackQuantity, RunBackPrice)
		SELECT     @ToTaskCode AS ToTaskCode, Quantity, TotalPrice, RunOnQuantity, RunOnPrice, RunBackQuantity, RunBackPrice
		FROM         Task.tbQuote 
		WHERE     (TaskCode = @FromTaskCode)
	
		INSERT INTO Task.tbOp
							  (TaskCode, OperationNumber, OpStatusCode, UserId, SyncTypeCode, Operation, Note, StartOn, EndOn, Duration, OffsetDays)
		SELECT     @ToTaskCode AS ToTaskCode, OperationNumber, 0 AS OpStatusCode, UserId, SyncTypeCode, Operation, Note, 
			CAST(CURRENT_TIMESTAMP AS date) AS StartOn, CAST(CURRENT_TIMESTAMP AS date) AS EndOn, Duration, OffsetDays
		FROM         Task.tbOp 
		WHERE     (TaskCode = @FromTaskCode)
	
		IF (ISNULL(@ParentTaskCode, '') = '')
			BEGIN
			IF EXISTS(SELECT     ParentTaskCode
					FROM         Task.tbFlow
					WHERE     (ChildTaskCode = @FromTaskCode))
				BEGIN
				SELECT @ParentTaskCode = ParentTaskCode
				FROM         Task.tbFlow
				WHERE     (ChildTaskCode = @FromTaskCode)

				SELECT @StepNumber = MAX(StepNumber)
				FROM         Task.tbFlow
				WHERE     (ParentTaskCode = @ParentTaskCode)
				GROUP BY ParentTaskCode
				
				SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10	
						
				INSERT INTO Task.tbFlow
				(ParentTaskCode, StepNumber, ChildTaskCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
				SELECT TOP 1 ParentTaskCode, @StepNumber AS Step, @ToTaskCode AS ChildTask, SyncTypeCode, UsedOnQuantity, OffsetDays
				FROM         Task.tbFlow
				WHERE     (ChildTaskCode = @FromTaskCode)
				END
			END
		ELSE
			BEGIN		
			INSERT INTO Task.tbFlow
			(ParentTaskCode, StepNumber, ChildTaskCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
			SELECT TOP 1 @ParentTaskCode As ParentTask, StepNumber, @ToTaskCode AS ChildTask, SyncTypeCode, UsedOnQuantity, OffsetDays
			FROM         Task.tbFlow 
			WHERE     (ChildTaskCode = @FromTaskCode)		
			END
	
		DECLARE curTask cursor local for			
			SELECT     ChildTaskCode
			FROM         Task.tbFlow
			WHERE     (ParentTaskCode = @FromTaskCode)
	
		OPEN curTask
	
		FETCH NEXT FROM curTask INTO @ChildTaskCode
		WHILE (@@FETCH_STATUS = 0)
			BEGIN
			EXEC Task.proc_Copy @ChildTaskCode, @ToTaskCode
			FETCH NEXT FROM curTask INTO @ChildTaskCode
			END
		
		CLOSE curTask
		DEALLOCATE curTask
		
		IF @@NESTLEVEL = 1
			BEGIN
			COMMIT TRANSACTION
			EXEC Task.proc_Schedule @ToTaskCode
			END

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
