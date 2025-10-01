CREATE   PROCEDURE Task.proc_Configure (@ParentTaskCode nvarchar(20))
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@StepNumber smallint
			, @TaskCode nvarchar(20)
			, @UserId nvarchar(10)
			, @ActivityCode nvarchar(50)
			, @AccountCode nvarchar(10)
			, @DefaultAccountCode nvarchar(10)
			, @TaxCode nvarchar(10)

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION

		INSERT INTO Org.tbContact 
			(AccountCode, ContactName, FileAs, PhoneNumber, EmailAddress)
		SELECT Task.tbTask.AccountCode, Task.tbTask.ContactName, Task.tbTask.ContactName AS NickName, Org.tbOrg.PhoneNumber, Org.tbOrg.EmailAddress
		FROM  Task.tbTask 
			INNER JOIN Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode
		WHERE LEN(ISNULL(Task.tbTask.ContactName, '')) > 0 AND (Task.tbTask.TaskCode = @ParentTaskCode)
					AND EXISTS (SELECT *
								FROM Task.tbTask
								WHERE (TaskCode = @ParentTaskCode) AND (NOT (ContactName IS NULL)) OR (TaskCode = @ParentTaskCode) AND (ContactName <> N''))
				AND NOT EXISTS(SELECT *
								FROM  Task.tbTask 
									INNER JOIN Org.tbContact ON Task.tbTask.AccountCode = Org.tbContact.AccountCode AND Task.tbTask.ContactName = Org.tbContact.ContactName
								WHERE     ( Task.tbTask.TaskCode = @ParentTaskCode))
	
		UPDATE Org.tbOrg
		SET OrganisationStatusCode = 1
		FROM Org.tbOrg INNER JOIN Task.tbTask ON Org.tbOrg.AccountCode = Task.tbTask.AccountCode
		WHERE ( Task.tbTask.TaskCode = @ParentTaskCode) AND ( Org.tbOrg.OrganisationStatusCode = 0)				
			AND EXISTS(SELECT *
				FROM  Org.tbOrg INNER JOIN Task.tbTask ON Org.tbOrg.AccountCode = Task.tbTask.AccountCode
				WHERE     ( Task.tbTask.TaskCode = @ParentTaskCode) AND ( Org.tbOrg.OrganisationStatusCode = 0))
	          
		UPDATE    Task.tbTask
		SET  ActionedOn = ActionOn
		WHERE (TaskCode = @ParentTaskCode)
			AND EXISTS(SELECT *
					  FROM Task.tbTask
					  WHERE (TaskStatusCode = 2) AND (TaskCode = @ParentTaskCode))

		UPDATE Task.tbTask
		SET TaskTitle = ActivityCode
		WHERE (TaskCode = @ParentTaskCode)
			AND EXISTS(SELECT *
				  FROM Task.tbTask
				  WHERE (TaskCode = @ParentTaskCode) AND (TaskTitle IS NULL))  	 				              
	     	
		INSERT INTO Task.tbAttribute
			(TaskCode, Attribute, AttributeDescription, PrintOrder, AttributeTypeCode)
		SELECT Task.tbTask.TaskCode, Activity.tbAttribute.Attribute, Activity.tbAttribute.DefaultText, Activity.tbAttribute.PrintOrder, Activity.tbAttribute.AttributeTypeCode
		FROM Activity.tbAttribute 
			INNER JOIN Task.tbTask ON Activity.tbAttribute.ActivityCode = Task.tbTask.ActivityCode
		WHERE     ( Task.tbTask.TaskCode = @ParentTaskCode)
	
		INSERT INTO Task.tbOp
			(TaskCode, UserId, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays, StartOn)
		SELECT Task.tbTask.TaskCode, Task.tbTask.UserId, Activity.tbOp.OperationNumber, Activity.tbOp.SyncTypeCode, Activity.tbOp.Operation, Activity.tbOp.Duration,  Activity.tbOp.OffsetDays, Task.tbTask.ActionOn
		FROM Activity.tbOp INNER JOIN Task.tbTask ON Activity.tbOp.ActivityCode = Task.tbTask.ActivityCode
		WHERE     ( Task.tbTask.TaskCode = @ParentTaskCode)
	                   
	
		SELECT @UserId = UserId FROM Task.tbTask WHERE Task.tbTask.TaskCode = @ParentTaskCode
	
		DECLARE curAct cursor local for
			SELECT Activity.tbFlow.StepNumber
			FROM Activity.tbFlow INNER JOIN Task.tbTask ON Activity.tbFlow.ParentCode = Task.tbTask.ActivityCode
			WHERE     ( Task.tbTask.TaskCode = @ParentTaskCode)
			ORDER BY Activity.tbFlow.StepNumber	
	
		OPEN curAct
		FETCH NEXT FROM curAct INTO @StepNumber
		WHILE @@FETCH_STATUS = 0
			BEGIN
			SELECT  
				@ActivityCode = Activity.tbActivity.ActivityCode, 
				@AccountCode = Task.tbTask.AccountCode
			FROM Activity.tbFlow 
				INNER JOIN Activity.tbActivity ON Activity.tbFlow.ChildCode = Activity.tbActivity.ActivityCode 
				INNER JOIN Task.tbTask ON Activity.tbFlow.ParentCode = Task.tbTask.ActivityCode
			WHERE     ( Activity.tbFlow.StepNumber = @StepNumber) AND ( Task.tbTask.TaskCode = @ParentTaskCode)
		
			EXEC Task.proc_NextCode @ActivityCode, @TaskCode output

			INSERT INTO Task.tbTask
				(TaskCode, UserId, AccountCode, ContactName, ActivityCode, TaskStatusCode, ActionById, ActionOn, TaskNotes, Quantity, UnitCharge, AddressCodeFrom, AddressCodeTo, CashCode, Printed, TaskTitle)
			SELECT  @TaskCode AS NewTask, Task_tb1.UserId, Task_tb1.AccountCode, Task_tb1.ContactName, Activity.tbActivity.ActivityCode, Activity.tbActivity.TaskStatusCode, 
						Task_tb1.ActionById, Task_tb1.ActionOn, Activity.tbActivity.ActivityDescription, Task_tb1.Quantity * Activity.tbFlow.UsedOnQuantity AS Quantity,
						Activity.tbActivity.UnitCharge, Org.tbOrg.AddressCode AS AddressCodeFrom, Org.tbOrg.AddressCode AS AddressCodeTo, 
						tbActivity.CashCode, CASE WHEN Activity.tbActivity.Printed = 0 THEN 1 ELSE 0 END AS Printed, Task_tb1.TaskTitle
			FROM  Activity.tbFlow 
				INNER JOIN Activity.tbActivity ON Activity.tbFlow.ChildCode = Activity.tbActivity.ActivityCode 
				INNER JOIN Task.tbTask Task_tb1 ON Activity.tbFlow.ParentCode = Task_tb1.ActivityCode 
				INNER JOIN Org.tbOrg ON Task_tb1.AccountCode = Org.tbOrg.AccountCode
			WHERE     ( Activity.tbFlow.StepNumber = @StepNumber) AND ( Task_tb1.TaskCode = @ParentTaskCode)

			IF EXISTS (SELECT * FROM Task.tbTask 
							INNER JOIN  Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode 
							INNER JOIN App.tbTaxCode ON Org.tbOrg.TaxCode = App.tbTaxCode.TaxCode AND Org.tbOrg.TaxCode = App.tbTaxCode.TaxCode)
				BEGIN
				UPDATE Task.tbTask
				SET TaxCode = App.tbTaxCode.TaxCode
				FROM Task.tbTask 
					INNER JOIN Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode 
					INNER JOIN App.tbTaxCode ON Org.tbOrg.TaxCode = App.tbTaxCode.TaxCode AND Org.tbOrg.TaxCode = App.tbTaxCode.TaxCode
				WHERE (Task.tbTask.TaskCode = @TaskCode)
				END
			ELSE
				BEGIN
				UPDATE Task.tbTask
				SET TaxCode = Cash.tbCode.TaxCode
				FROM  Task.tbTask 
					INNER JOIN Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode
				WHERE  (Task.tbTask.TaskCode = @TaskCode)
				END			
			
			SELECT @DefaultAccountCode = (SELECT TOP 1  AccountCode FROM Task.tbTask
											WHERE   (ActivityCode = (SELECT ActivityCode FROM  Task.tbTask AS tbTask_1 WHERE (TaskCode = @TaskCode))) AND (TaskCode <> @TaskCode))

			IF NOT @DefaultAccountCode IS NULL
				BEGIN
				UPDATE Task.tbTask
				SET AccountCode = @DefaultAccountCode
				WHERE (TaskCode = @TaskCode)
				END
					
			INSERT INTO Task.tbFlow
				(ParentTaskCode, StepNumber, ChildTaskCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
			SELECT Task.tbTask.TaskCode, Activity.tbFlow.StepNumber, @TaskCode AS ChildTaskCode, Activity.tbFlow.SyncTypeCode, Activity.tbFlow.UsedOnQuantity, Activity.tbFlow.OffsetDays
			FROM Activity.tbFlow 
				INNER JOIN Task.tbTask ON Activity.tbFlow.ParentCode = Task.tbTask.ActivityCode
			WHERE (Task.tbTask.TaskCode = @ParentTaskCode) AND ( Activity.tbFlow.StepNumber = @StepNumber)
		
			EXEC Task.proc_Configure @TaskCode

			FETCH NEXT FROM curAct INTO @StepNumber
			END
	
		CLOSE curAct
		DEALLOCATE curAct
		
		IF @@NESTLEVEL = 1
			COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
