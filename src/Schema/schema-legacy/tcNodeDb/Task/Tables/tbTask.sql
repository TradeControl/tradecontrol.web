CREATE TABLE [Task].[tbTask] (
    [TaskCode]        NVARCHAR (20)   NOT NULL,
    [UserId]          NVARCHAR (10)   NOT NULL,
    [AccountCode]     NVARCHAR (10)   NOT NULL,
    [SecondReference] NVARCHAR (20)   NULL,
    [TaskTitle]       NVARCHAR (100)  NULL,
    [ContactName]     NVARCHAR (100)  NULL,
    [ActivityCode]    NVARCHAR (50)   NOT NULL,
    [TaskStatusCode]  SMALLINT        NOT NULL,
    [ActionById]      NVARCHAR (10)   NOT NULL,
    [ActionOn]        DATETIME        CONSTRAINT [DF_Task_tbTask_ActionOn] DEFAULT (getdate()) NOT NULL,
    [ActionedOn]      DATETIME        NULL,
    [PaymentOn]       DATETIME        CONSTRAINT [DF_Task_tb_PaymentOn] DEFAULT (getdate()) NOT NULL,
    [TaskNotes]       NVARCHAR (255)  NULL,
    [CashCode]        NVARCHAR (50)   NULL,
    [TaxCode]         NVARCHAR (10)   NULL,
    [AddressCodeFrom] NVARCHAR (15)   NULL,
    [AddressCodeTo]   NVARCHAR (15)   NULL,
    [Spooled]         BIT             CONSTRAINT [DF_Task_tb_Spooled] DEFAULT ((0)) NOT NULL,
    [Printed]         BIT             CONSTRAINT [DF_Task_tb_Printed] DEFAULT ((0)) NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Task_tb_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Task_tb_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Task_tb_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Task_tb_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Quantity]        DECIMAL (18, 4) CONSTRAINT [DF_Task_tb_Quantity] DEFAULT ((0)) NOT NULL,
    [TotalCharge]     DECIMAL (18, 5) CONSTRAINT [DF_Task_tb_TotalCharge] DEFAULT ((0)) NOT NULL,
    [UnitCharge]      DECIMAL (18, 7) CONSTRAINT [DF_Task_tb_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Task_tbTask] PRIMARY KEY CLUSTERED ([TaskCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [Activity_tb_FK00] FOREIGN KEY ([ActivityCode]) REFERENCES [Activity].[tbActivity] ([ActivityCode]) ON UPDATE CASCADE,
    CONSTRAINT [Activity_tb_FK01] FOREIGN KEY ([TaskStatusCode]) REFERENCES [Task].[tbStatus] ([TaskStatusCode]),
    CONSTRAINT [Activity_tb_FK02] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Task_tb_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]),
    CONSTRAINT [FK_Task_tb_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]),
    CONSTRAINT [FK_Task_tb_Org_tbAddress_From] FOREIGN KEY ([AddressCodeFrom]) REFERENCES [Org].[tbAddress] ([AddressCode]),
    CONSTRAINT [FK_Task_tb_Org_tbAddress_To] FOREIGN KEY ([AddressCodeTo]) REFERENCES [Org].[tbAddress] ([AddressCode]),
    CONSTRAINT [FK_Task_tb_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Task_tb_Usr_tb_ActionById] FOREIGN KEY ([ActionById]) REFERENCES [Usr].[tbUser] ([UserId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_AccountCode]
    ON [Task].[tbTask]([AccountCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_AccountCodeByActionOn]
    ON [Task].[tbTask]([AccountCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_AccountCodeByStatus]
    ON [Task].[tbTask]([AccountCode] ASC, [TaskStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_ActionBy]
    ON [Task].[tbTask]([ActionById] ASC, [TaskStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_ActionById]
    ON [Task].[tbTask]([ActionById] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_ActionOn]
    ON [Task].[tbTask]([ActionOn] DESC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_ActionOnStatus]
    ON [Task].[tbTask]([TaskStatusCode] ASC, [ActionOn] ASC, [AccountCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_ActivityCode]
    ON [Task].[tbTask]([ActivityCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_ActivityCodeTaskTitle]
    ON [Task].[tbTask]([ActivityCode] ASC, [TaskTitle] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_CashCode]
    ON [Task].[tbTask]([CashCode] ASC, [TaskStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_TaskStatusCode]
    ON [Task].[tbTask]([TaskStatusCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tb_UserId]
    ON [Task].[tbTask]([UserId] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbTask_ActionOn_Status_CashCode]
    ON [Task].[tbTask]([ActionOn] ASC, [TaskStatusCode] ASC, [CashCode] ASC, [TaskCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbTask_ActionOn_TaskCode_CashCode]
    ON [Task].[tbTask]([ActionOn] ASC, [TaskCode] ASC, [CashCode] ASC, [TaskStatusCode] ASC, [AccountCode] ASC)
    INCLUDE([TaskTitle], [ActivityCode], [ActionedOn], [Quantity], [UnitCharge], [TotalCharge], [PaymentOn]);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbTask_Status_TaxCode_TaskCode]
    ON [Task].[tbTask]([TaskStatusCode] ASC, [TaxCode] ASC, [TaskCode] ASC, [CashCode] ASC, [ActionOn] ASC)
    INCLUDE([TotalCharge]);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbTask_TaskCode_CashCode]
    ON [Task].[tbTask]([TaskCode] ASC, [CashCode] ASC)
    INCLUDE([Quantity], [UnitCharge]);


GO
CREATE NONCLUSTERED INDEX [IX_Task_tbTask_TaskCode_TaxCode_CashCode]
    ON [Task].[tbTask]([TaskCode] ASC, [TaxCode] ASC, [CashCode] ASC, [ActionOn] ASC)
    INCLUDE([TotalCharge]);


GO
CREATE TRIGGER Task.Task_tbTask_TriggerInsert
ON Task.tbTask
FOR INSERT
AS
	SET NOCOUNT ON;
	BEGIN TRY

	    UPDATE task
	    SET task.ActionOn = CAST(task.ActionOn AS DATE)
	    FROM Task.tbTask task JOIN inserted i ON task.TaskCode = i.TaskCode
	    WHERE DATEDIFF(SECOND, CAST(i.ActionOn AS DATE), i.ActionOn) <> 0;

	    UPDATE task
	    SET task.TotalCharge = i.UnitCharge * i.Quantity
	    FROM Task.tbTask task JOIN inserted i ON task.TaskCode = i.TaskCode
	    WHERE i.TotalCharge = 0 

	    UPDATE task
	    SET task.UnitCharge = i.TotalCharge / i.Quantity
	    FROM Task.tbTask task JOIN inserted i ON task.TaskCode = i.TaskCode
	    WHERE i.UnitCharge = 0 AND i.Quantity > 0;

	    UPDATE task
	    SET PaymentOn = App.fnAdjustToCalendar(
            CASE WHEN org.PayDaysFromMonthEnd <> 0 THEN 
                    DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, org.PaymentDays + org.ExpectedDays, task.ActionOn), 'yyyyMM'), '01')))												
                ELSE 
                    DATEADD(d, org.PaymentDays + org.ExpectedDays, task.ActionOn) END, 0) 
	    FROM Task.tbTask task
		    JOIN Org.tbOrg org ON task.AccountCode = org.AccountCode
		    JOIN inserted i ON task.TaskCode = i.TaskCode
	    WHERE NOT task.CashCode IS NULL 

	    INSERT INTO Org.tbContact (AccountCode, ContactName)
	    SELECT DISTINCT AccountCode, ContactName 
	    FROM inserted
	    WHERE EXISTS (SELECT ContactName FROM inserted AS i WHERE (NOT (ContactName IS NULL)) AND (ContactName <> N''))
                AND NOT EXISTS(SELECT Org.tbContact.ContactName FROM inserted AS i INNER JOIN Org.tbContact ON i.AccountCode = Org.tbContact.AccountCode AND i.ContactName = Org.tbContact.ContactName)

		INSERT INTO Task.tbChangeLog
								 (TaskCode, TransmitStatusCode, AccountCode, ActivityCode, TaskStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
		SELECT inserted.TaskCode, Org.tbOrg.TransmitStatusCode, inserted.AccountCode, inserted.ActivityCode, inserted.TaskStatusCode, 
								 inserted.ActionOn, inserted.Quantity, inserted.CashCode, inserted.TaxCode, inserted.UnitCharge
		FROM inserted 
			JOIN Org.tbOrg ON inserted.AccountCode = Org.tbOrg.AccountCode
			JOIN Cash.tbCode ON inserted.CashCode = Cash.tbCode.CashCode
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		EXEC App.proc_ErrorLog;
		THROW;
	END CATCH

GO
CREATE TRIGGER Task.Task_tbTask_TriggerUpdate
ON Task.tbTask
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE task
		SET task.ActionOn = CAST(task.ActionOn AS DATE)
		FROM Task.tbTask task JOIN inserted i ON task.TaskCode = i.TaskCode
		WHERE DATEDIFF(SECOND, CAST(i.ActionOn AS DATE), i.ActionOn) <> 0;

		IF UPDATE(TaskStatusCode)
		BEGIN
			UPDATE ops
			SET OpStatusCode = 2
			FROM inserted JOIN Task.tbOp ops ON inserted.TaskCode = ops.TaskCode
			WHERE TaskStatusCode > 1 AND OpStatusCode < 2;

			WITH first_ops AS
			(
				SELECT ops.TaskCode, MIN(ops.OperationNumber) AS OperationNumber
				FROM inserted i JOIN Task.tbOp ops ON i.TaskCode = ops.TaskCode 
				WHERE i.TaskStatusCode = 1		
				GROUP BY ops.TaskCode		
			), next_ops AS
			(
				SELECT ops.TaskCode, ops.OperationNumber, ops.SyncTypeCode,
					LEAD(ops.OperationNumber) OVER (PARTITION BY ops.TaskCode ORDER BY ops.OperationNumber) AS NextOpNo
				FROM inserted i JOIN Task.tbOp ops ON i.TaskCode = ops.TaskCode 
			), async_ops AS
			(
				SELECT first_ops.TaskCode, first_ops.OperationNumber, next_ops.NextOpNo
				FROM first_ops JOIN next_ops ON first_ops.TaskCode = next_ops.TaskCode AND first_ops.OperationNumber = next_ops.OperationNumber

				UNION ALL

				SELECT next_ops.TaskCode, next_ops.OperationNumber, next_ops.NextOpNo
				FROM next_ops JOIN async_ops ON next_ops.TaskCode = async_ops.TaskCode AND next_ops.OperationNumber = async_ops.NextOpNo
				WHERE next_ops.SyncTypeCode = 1

			)
			UPDATE ops
			SET OpStatusCode = 1
			FROM async_ops JOIN Task.tbOp ops ON async_ops.TaskCode = ops.TaskCode
				AND async_ops.OperationNumber = ops.OperationNumber;
			
			WITH cascade_status AS
			(
				SELECT TaskCode, TaskStatusCode
				FROM Task.tbTask inserted
				WHERE NOT CashCode IS NULL
			), task_flow AS
			(
				SELECT cascade_status.TaskStatusCode ParentStatusCode, child.ParentTaskCode, child.ChildTaskCode, child_task.TaskStatusCode
				FROM Task.tbFlow child 
					JOIN cascade_status ON child.ParentTaskCode = cascade_status.TaskCode
					JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode

				UNION ALL

				SELECT parent.ParentStatusCode, child.ParentTaskCode, child.ChildTaskCode, child_task.TaskStatusCode
				FROM Task.tbFlow child 
					JOIN task_flow parent ON child.ParentTaskCode = parent.ChildTaskCode
					JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode
			)
			UPDATE task
			SET TaskStatusCode = CASE task_flow.ParentStatusCode WHEN 3 THEN 2 ELSE task_flow.ParentStatusCode END
			FROM Task.tbTask task JOIN task_flow ON task_flow.ChildTaskCode = task.TaskCode
			WHERE task.TaskStatusCode < 2;

			--not triggering fix
			WITH cascade_status AS
			(
				SELECT TaskCode, TaskStatusCode
				FROM Task.tbTask inserted
				WHERE NOT CashCode IS NULL AND TaskStatusCode > 1
			), task_flow AS
			(
				SELECT cascade_status.TaskStatusCode ParentStatusCode, child.ParentTaskCode, child.ChildTaskCode, child_task.TaskStatusCode
				FROM Task.tbFlow child 
					JOIN cascade_status ON child.ParentTaskCode = cascade_status.TaskCode
					JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode
				WHERE CashCode IS NULL

				UNION ALL

				SELECT parent.ParentStatusCode, child.ParentTaskCode, child.ChildTaskCode, child_task.TaskStatusCode
				FROM Task.tbFlow child 
					JOIN task_flow parent ON child.ParentTaskCode = parent.ChildTaskCode
					JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode
				WHERE CashCode IS NULL
			)
			UPDATE ops
			SET OpStatusCode = 2
			FROM Task.tbOp ops JOIN task_flow ON task_flow.ChildTaskCode = ops.TaskCode
			WHERE ops.OpStatusCode < 2;

			DELETE cost_set 
			FROM inserted 
				JOIN Task.tbCostSet cost_set ON inserted.TaskCode = cost_set.TaskCode
			WHERE inserted.TaskStatusCode > 0;
			
		END

		IF UPDATE(Quantity)
		BEGIN
			UPDATE flow
			SET UsedOnQuantity = inserted.Quantity / parent_task.Quantity
			FROM Task.tbFlow AS flow 
				JOIN inserted ON flow.ChildTaskCode = inserted.TaskCode 
				JOIN Task.tbTask AS parent_task ON flow.ParentTaskCode = parent_task.TaskCode
				JOIN Cash.tbCode ON parent_task.CashCode = Cash.tbCode.CashCode
			WHERE (flow.UsedOnQuantity <> 0) AND (inserted.Quantity <> 0) 
				AND (inserted.Quantity / parent_task.Quantity <> flow.UsedOnQuantity)
		END

		IF UPDATE(Quantity) OR UPDATE(UnitCharge)
		BEGIN
			UPDATE task
			SET task.TotalCharge = i.Quantity * i.UnitCharge
			FROM Task.tbTask task JOIN inserted i ON task.TaskCode = i.TaskCode
		END

		IF UPDATE(TotalCharge)
		BEGIN
			UPDATE task
			SET task.UnitCharge = CASE i.TotalCharge + i.Quantity WHEN 0 THEN 0 ELSE i.TotalCharge / i.Quantity END
			FROM Task.tbTask task JOIN inserted i ON task.TaskCode = i.TaskCode			
		END

		IF UPDATE(ActionOn)
		BEGIN			
			WITH parent_task AS
			(
				SELECT        ParentTaskCode
				FROM            Task.tbFlow flow
					JOIN Task.tbTask task ON flow.ParentTaskCode = task.TaskCode
					JOIN Cash.tbCode cash ON task.CashCode = cash.CashCode
					JOIN inserted ON flow.ChildTaskCode = inserted.TaskCode
				--manual scheduling only
				WHERE (SELECT SUM(UsedOnQuantity) FROM inserted JOIN Task.tbFlow ON inserted.TaskCode = Task.tbFlow.ChildTaskCode) = 0	
			), task_flow AS
			(
				SELECT        flow.ParentTaskCode, flow.StepNumber, task.ActionOn,
						LAG(task.ActionOn, 1, task.ActionOn) OVER (PARTITION BY flow.ParentTaskCode ORDER BY StepNumber) AS PrevActionOn
				FROM Task.tbFlow flow
					JOIN Task.tbTask task ON flow.ChildTaskCode = task.TaskCode
					JOIN parent_task ON flow.ParentTaskCode = parent_task.ParentTaskCode
			), step_disordered AS
			(
				SELECT ParentTaskCode 
				FROM task_flow
				WHERE ActionOn < PrevActionOn
				GROUP BY ParentTaskCode
			), step_ordered AS
			(
				SELECT flow.ParentTaskCode, flow.ChildTaskCode,
					ROW_NUMBER() OVER (PARTITION BY flow.ParentTaskCode ORDER BY task.ActionOn, flow.StepNumber) * 10 AS StepNumber 
				FROM step_disordered
					JOIN Task.tbFlow flow ON step_disordered.ParentTaskCode = flow.ParentTaskCode
					JOIN Task.tbTask task ON flow.ChildTaskCode = task.TaskCode
			)
			UPDATE flow
			SET
				StepNumber = step_ordered.StepNumber
			FROM Task.tbFlow flow
				JOIN step_ordered ON flow.ParentTaskCode = step_ordered.ParentTaskCode AND flow.ChildTaskCode = step_ordered.ChildTaskCode;
			
			IF EXISTS(SELECT * FROM App.tbOptions WHERE IsAutoOffsetDays <> 0)
			BEGIN
				UPDATE flow
				SET OffsetDays = App.fnOffsetDays(inserted.ActionOn, parent_task.ActionOn)
									- ISNULL((SELECT SUM(OffsetDays) FROM Task.tbFlow sub_flow WHERE sub_flow.ParentTaskCode = flow.ParentTaskCode AND sub_flow.StepNumber > flow.StepNumber), 0)
				FROM Task.tbFlow AS flow 
					JOIN inserted ON flow.ChildTaskCode = inserted.TaskCode 
					JOIN Task.tbTask AS parent_task ON flow.ParentTaskCode = parent_task.TaskCode
					JOIN Cash.tbCode ON parent_task.CashCode = Cash.tbCode.CashCode
				WHERE (SELECT SUM(UsedOnQuantity) FROM inserted JOIN Task.tbFlow ON inserted.TaskCode = Task.tbFlow.ChildTaskCode) = 0
			END

			UPDATE task
			SET PaymentOn = App.fnAdjustToCalendar(CASE WHEN org.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, org.PaymentDays + org.ExpectedDays, i.ActionOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, org.PaymentDays + org.ExpectedDays, i.ActionOn)	
													END, 0) 
			FROM Task.tbTask task
				JOIN inserted i ON task.TaskCode = i.TaskCode
				JOIN Org.tbOrg org ON i.AccountCode = org.AccountCode				
			WHERE NOT task.CashCode IS NULL 
		END

		IF UPDATE (TaskTitle)
		BEGIN
			WITH cascade_title_change AS
			(
				SELECT inserted.TaskCode, inserted.TaskTitle AS TaskTitle, deleted.TaskTitle AS PreviousTitle 				
				FROM inserted
					JOIN deleted ON inserted.TaskCode = deleted.TaskCode
			), task_flow AS
			(
				SELECT cascade_title_change.TaskTitle AS ProjectTitle, cascade_title_change.PreviousTitle, child.ParentTaskCode, child.ChildTaskCode, child_task.TaskTitle
				FROM Task.tbFlow child 
					JOIN cascade_title_change ON child.ParentTaskCode = cascade_title_change.TaskCode
					JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode

				UNION ALL

				SELECT parent.ProjectTitle, parent.PreviousTitle, child.ParentTaskCode, child.ChildTaskCode, child_task.TaskTitle
				FROM Task.tbFlow child 
					JOIN task_flow parent ON child.ParentTaskCode = parent.ChildTaskCode
					JOIN Task.tbTask child_task ON child.ChildTaskCode = child_task.TaskCode
			)
			UPDATE task
			SET TaskTitle = ProjectTitle
			FROM Task.tbTask task JOIN task_flow ON task.TaskCode = task_flow.ChildTaskCode
			WHERE task_flow.PreviousTitle = task_flow.TaskTitle;
		END

		IF UPDATE (Spooled)
		BEGIN
			INSERT INTO App.tbDocSpool (DocTypeCode, DocumentNumber)
			SELECT CASE 
					WHEN CashModeCode = 0 THEN		--Expense
						CASE WHEN TaskStatusCode = 0 THEN 2	ELSE 3 END	--Enquiry								
					WHEN CashModeCode = 1 THEN		--Income
						CASE WHEN TaskStatusCode = 0 THEN 0	ELSE 1 END	--Quote
					END AS DocTypeCode, task.TaskCode
			FROM   inserted task INNER JOIN
									 Cash.tbCode ON task.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE (task.Spooled <> 0)
				
			DELETE App.tbDocSpool
			FROM         inserted i INNER JOIN
								  App.tbDocSpool ON i.TaskCode = App.tbDocSpool.DocumentNumber
			WHERE    (i.Spooled = 0) AND ( App.tbDocSpool.DocTypeCode <= 3)
		END

		IF UPDATE (ContactName)
		BEGIN
			INSERT INTO Org.tbContact (AccountCode, ContactName)
			SELECT DISTINCT AccountCode, ContactName FROM inserted
			WHERE EXISTS (SELECT     *
						FROM         inserted AS i
						WHERE     (NOT (ContactName IS NULL)) AND
												(ContactName <> N''))
				AND NOT EXISTS(SELECT  *
								FROM         inserted AS i INNER JOIN
													Org.tbContact ON i.AccountCode = Org.tbContact.AccountCode AND i.ContactName = Org.tbContact.ContactName)
		END
		
		UPDATE Task.tbTask
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Task.tbTask INNER JOIN inserted AS i ON tbTask.TaskCode = i.TaskCode;

		IF UPDATE(TaskStatusCode) OR UPDATE(Quantity) OR UPDATE(ActionOn) OR UPDATE(UnitCharge) OR UPDATE(ActivityCode) OR UPDATE(CashCode) OR UPDATE (TaxCode)
		BEGIN
			WITH candidates AS
			(
				SELECT TaskCode, AccountCode, ActivityCode, TaskStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge
				FROM inserted
				WHERE EXISTS (SELECT * FROM Task.tbChangeLog WHERE TaskCode = inserted.TaskCode)
			)
			, logs AS
			(
				SELECT clog.LogId, clog.TaskCode, clog.AccountCode, clog.ActivityCode, clog.TaskStatusCode, clog.TransmitStatusCode, clog.ActionOn, clog.Quantity, clog.CashCode, clog.TaxCode, clog.UnitCharge
				FROM Task.tbChangeLog clog
				JOIN candidates ON clog.TaskCode = candidates.TaskCode AND LogId = (SELECT MAX(LogId) FROM Task.tbChangeLog WHERE TaskCode = candidates.TaskCode)		
			)
			INSERT INTO Task.tbChangeLog
									(TaskCode, TransmitStatusCode, AccountCode, ActivityCode, TaskStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
			SELECT candidates.TaskCode, CASE orgs.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, candidates.AccountCode,
				candidates.ActivityCode, candidates.TaskStatusCode, candidates.ActionOn, candidates.Quantity, candidates.CashCode, candidates.TaxCode, candidates.UnitCharge
			FROM candidates 
				JOIN Org.tbOrg orgs ON candidates.AccountCode = orgs.AccountCode 
				JOIN logs ON candidates.TaskCode = logs.TaskCode
			WHERE (logs.TaskStatusCode <> candidates.TaskStatusCode) 
				OR (logs.TransmitStatusCode < 2)
				OR (logs.ActionOn <> candidates.ActionOn) 
				OR (logs.Quantity <> candidates.Quantity)
				OR (logs.UnitCharge <> candidates.UnitCharge)
				OR (logs.TaxCode <> candidates.TaxCode);
		END;

		IF UPDATE(AccountCode)
		BEGIN
			WITH candidates AS
			(
				SELECT inserted.* FROM inserted
				JOIN deleted ON inserted.TaskCode = deleted.TaskCode
				WHERE inserted.AccountCode <> deleted.AccountCode
			)
			INSERT INTO Task.tbChangeLog
									 (TaskCode, TransmitStatusCode, AccountCode, ActivityCode, TaskStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
			SELECT candidates.TaskCode, Org.tbOrg.TransmitStatusCode, candidates.AccountCode, candidates.ActivityCode, candidates.TaskStatusCode, 
									 candidates.ActionOn, candidates.Quantity, candidates.CashCode, candidates.TaxCode, candidates.UnitCharge
			FROM candidates 
				JOIN Org.tbOrg ON candidates.AccountCode = Org.tbOrg.AccountCode
				JOIN Cash.tbCode ON candidates.CashCode = Cash.tbCode.CashCode;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE   TRIGGER Task.Task_tbTask_TriggerDelete
ON Task.tbTask
FOR DELETE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO Task.tbChangeLog
								 (TaskCode, TransmitStatusCode, AccountCode, ActivityCode, TaskStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
		SELECT deleted.TaskCode, CASE Org.tbOrg.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, 
					deleted.AccountCode, deleted.ActivityCode, 4 CancelledStatusCode, 
					deleted.ActionOn, deleted.Quantity, deleted.CashCode, deleted.TaxCode, deleted.UnitCharge
		FROM deleted INNER JOIN Org.tbOrg ON deleted.AccountCode = Org.tbOrg.AccountCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
