CREATE TABLE [Project].[tbProject] (
    [ProjectCode]        NVARCHAR (20)   NOT NULL,
    [UserId]          NVARCHAR (10)   NOT NULL,
    [SubjectCode]     NVARCHAR (10)   NOT NULL,
    [SecondReference] NVARCHAR (20)   NULL,
    [ProjectTitle]       NVARCHAR (100)  NULL,
    [ContactName]     NVARCHAR (100)  NULL,
    [ObjectCode]    NVARCHAR (50)   NOT NULL,
    [ProjectStatusCode]  SMALLINT        NOT NULL,
    [ActionById]      NVARCHAR (10)   NOT NULL,
    [ActionOn]        DATETIME        CONSTRAINT [DF_Project_tbProject_ActionOn] DEFAULT (getdate()) NOT NULL,
    [ActionedOn]      DATETIME        NULL,
    [PaymentOn]       DATETIME        CONSTRAINT [DF_Project_tb_PaymentOn] DEFAULT (getdate()) NOT NULL,
    [ProjectNotes]       NVARCHAR (255)  NULL,
    [CashCode]        NVARCHAR (50)   NULL,
    [TaxCode]         NVARCHAR (10)   NULL,
    [AddressCodeFrom] NVARCHAR (15)   NULL,
    [AddressCodeTo]   NVARCHAR (15)   NULL,
    [Spooled]         BIT             CONSTRAINT [DF_Project_tb_Spooled] DEFAULT ((0)) NOT NULL,
    [Printed]         BIT             CONSTRAINT [DF_Project_tb_Printed] DEFAULT ((0)) NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Project_tb_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Project_tb_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Project_tb_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Project_tb_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Quantity]        DECIMAL (18, 4) CONSTRAINT [DF_Project_tb_Quantity] DEFAULT ((0)) NOT NULL,
    [TotalCharge]     DECIMAL (18, 5) CONSTRAINT [DF_Project_tb_TotalCharge] DEFAULT ((0)) NOT NULL,
    [UnitCharge]      DECIMAL (18, 7) CONSTRAINT [DF_Project_tb_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Project_tbProject] PRIMARY KEY CLUSTERED ([ProjectCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Project_tb_tbObject] FOREIGN KEY ([ObjectCode]) REFERENCES [Object].[tbObject] ([ObjectCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Project_tb_tbStatus] FOREIGN KEY ([ProjectStatusCode]) REFERENCES [Project].[tbStatus] ([ProjectStatusCode]),
    CONSTRAINT [FK_Project_tb_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Project_tb_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]),
    CONSTRAINT [FK_Project_tb_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]),
    CONSTRAINT [FK_Project_tb_Subject_tbAddress_From] FOREIGN KEY ([AddressCodeFrom]) REFERENCES [Subject].[tbAddress] ([AddressCode]),
    CONSTRAINT [FK_Project_tb_Subject_tbAddress_To] FOREIGN KEY ([AddressCodeTo]) REFERENCES [Subject].[tbAddress] ([AddressCode]),
    CONSTRAINT [FK_Project_tb_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Project_tb_Usr_tb_ActionById] FOREIGN KEY ([ActionById]) REFERENCES [Usr].[tbUser] ([UserId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_AccountCode]
    ON [Project].[tbProject]([SubjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_AccountCodeByActionOn]
    ON [Project].[tbProject]([SubjectCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_AccountCodeByStatus]
    ON [Project].[tbProject]([SubjectCode] ASC, [ProjectStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_ActionBy]
    ON [Project].[tbProject]([ActionById] ASC, [ProjectStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_ActionById]
    ON [Project].[tbProject]([ActionById] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_ActionOn]
    ON [Project].[tbProject]([ActionOn] DESC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_ActionOnStatus]
    ON [Project].[tbProject]([ProjectStatusCode] ASC, [ActionOn] ASC, [SubjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_ObjectCode]
    ON [Project].[tbProject]([ObjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_ObjectCodeProjectTitle]
    ON [Project].[tbProject]([ObjectCode] ASC, [ProjectTitle] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_CashCode]
    ON [Project].[tbProject]([CashCode] ASC, [ProjectStatusCode] ASC, [ActionOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_ProjectStatusCode]
    ON [Project].[tbProject]([ProjectStatusCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tb_UserId]
    ON [Project].[tbProject]([UserId] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_ActionOn_Status_CashCode]
    ON [Project].[tbProject]([ActionOn] ASC, [ProjectStatusCode] ASC, [CashCode] ASC, [ProjectCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_ActionOn_ProjectCode_CashCode]
    ON [Project].[tbProject]([ActionOn] ASC, [ProjectCode] ASC, [CashCode] ASC, [ProjectStatusCode] ASC, [SubjectCode] ASC)
    INCLUDE([ProjectTitle], [ObjectCode], [ActionedOn], [Quantity], [UnitCharge], [TotalCharge], [PaymentOn]);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_Status_TaxCode_ProjectCode]
    ON [Project].[tbProject]([ProjectStatusCode] ASC, [TaxCode] ASC, [ProjectCode] ASC, [CashCode] ASC, [ActionOn] ASC)
    INCLUDE([TotalCharge]);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_ProjectCode_CashCode]
    ON [Project].[tbProject]([ProjectCode] ASC, [CashCode] ASC)
    INCLUDE([Quantity], [UnitCharge]);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbProject_ProjectCode_TaxCode_CashCode]
    ON [Project].[tbProject]([ProjectCode] ASC, [TaxCode] ASC, [CashCode] ASC, [ActionOn] ASC)
    INCLUDE([TotalCharge]);


GO
CREATE TRIGGER Project.Project_tbProject_TriggerInsert
ON Project.tbProject
FOR INSERT
AS
	SET NOCOUNT ON;
	BEGIN TRY

	    UPDATE Project
	    SET Project.ActionOn = CAST(Project.ActionOn AS DATE)
	    FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
	    WHERE DATEDIFF(SECOND, CAST(i.ActionOn AS DATE), i.ActionOn) <> 0;

	    UPDATE Project
	    SET Project.TotalCharge = i.UnitCharge * i.Quantity
	    FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
	    WHERE i.TotalCharge = 0 

	    UPDATE Project
	    SET Project.UnitCharge = i.TotalCharge / i.Quantity
	    FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
	    WHERE i.UnitCharge = 0 AND i.Quantity > 0;

	    UPDATE Project
	    SET PaymentOn = App.fnAdjustToCalendar(
            CASE WHEN Subject.PayDaysFromMonthEnd <> 0 THEN 
                    DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, Project.ActionOn), 'yyyyMM'), '01')))												
                ELSE 
                    DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, Project.ActionOn) END, 0) 
	    FROM Project.tbProject Project
		    JOIN Subject.tbSubject Subject ON Project.SubjectCode = Subject.SubjectCode
		    JOIN inserted i ON Project.ProjectCode = i.ProjectCode
	    WHERE NOT Project.CashCode IS NULL 

	    INSERT INTO Subject.tbContact (SubjectCode, ContactName)
	    SELECT DISTINCT SubjectCode, ContactName 
	    FROM inserted
	    WHERE EXISTS (SELECT ContactName FROM inserted AS i WHERE (NOT (ContactName IS NULL)) AND (ContactName <> N''))
                AND NOT EXISTS(SELECT Subject.tbContact.ContactName FROM inserted AS i INNER JOIN Subject.tbContact ON i.SubjectCode = Subject.tbContact.SubjectCode AND i.ContactName = Subject.tbContact.ContactName)

		INSERT INTO Project.tbChangeLog
								 (ProjectCode, TransmitStatusCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
		SELECT inserted.ProjectCode, Subject.tbSubject.TransmitStatusCode, inserted.SubjectCode, inserted.ObjectCode, inserted.ProjectStatusCode, 
								 inserted.ActionOn, inserted.Quantity, inserted.CashCode, inserted.TaxCode, inserted.UnitCharge
		FROM inserted 
			JOIN Subject.tbSubject ON inserted.SubjectCode = Subject.tbSubject.SubjectCode
			JOIN Cash.tbCode ON inserted.CashCode = Cash.tbCode.CashCode
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		EXEC App.proc_ErrorLog;
		THROW;
	END CATCH

GO
CREATE TRIGGER Project.Project_tbProject_TriggerUpdate
ON Project.tbProject
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE Project
		SET Project.ActionOn = CAST(Project.ActionOn AS DATE)
		FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
		WHERE DATEDIFF(SECOND, CAST(i.ActionOn AS DATE), i.ActionOn) <> 0;

		IF UPDATE(ProjectStatusCode)
		BEGIN
			UPDATE ops
			SET OpStatusCode = 2
			FROM inserted JOIN Project.tbOp ops ON inserted.ProjectCode = ops.ProjectCode
			WHERE ProjectStatusCode > 1 AND OpStatusCode < 2;

			WITH first_ops AS
			(
				SELECT ops.ProjectCode, MIN(ops.OperationNumber) AS OperationNumber
				FROM inserted i JOIN Project.tbOp ops ON i.ProjectCode = ops.ProjectCode 
				WHERE i.ProjectStatusCode = 1		
				GROUP BY ops.ProjectCode		
			), next_ops AS
			(
				SELECT ops.ProjectCode, ops.OperationNumber, ops.SyncTypeCode,
					LEAD(ops.OperationNumber) OVER (PARTITION BY ops.ProjectCode ORDER BY ops.OperationNumber) AS NextOpNo
				FROM inserted i JOIN Project.tbOp ops ON i.ProjectCode = ops.ProjectCode 
			), async_ops AS
			(
				SELECT first_ops.ProjectCode, first_ops.OperationNumber, next_ops.NextOpNo
				FROM first_ops JOIN next_ops ON first_ops.ProjectCode = next_ops.ProjectCode AND first_ops.OperationNumber = next_ops.OperationNumber

				UNION ALL

				SELECT next_ops.ProjectCode, next_ops.OperationNumber, next_ops.NextOpNo
				FROM next_ops JOIN async_ops ON next_ops.ProjectCode = async_ops.ProjectCode AND next_ops.OperationNumber = async_ops.NextOpNo
				WHERE next_ops.SyncTypeCode = 1

			)
			UPDATE ops
			SET OpStatusCode = 1
			FROM async_ops JOIN Project.tbOp ops ON async_ops.ProjectCode = ops.ProjectCode
				AND async_ops.OperationNumber = ops.OperationNumber;
			
			WITH cascade_status AS
			(
				SELECT ProjectCode, ProjectStatusCode
				FROM Project.tbProject inserted
				WHERE NOT CashCode IS NULL
			), Project_flow AS
			(
				SELECT cascade_status.ProjectStatusCode ParentStatusCode, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectStatusCode
				FROM Project.tbFlow child 
					JOIN cascade_status ON child.ParentProjectCode = cascade_status.ProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode

				UNION ALL

				SELECT parent.ParentStatusCode, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectStatusCode
				FROM Project.tbFlow child 
					JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
			)
			UPDATE Project
			SET ProjectStatusCode = CASE Project_flow.ParentStatusCode WHEN 3 THEN 2 ELSE Project_flow.ParentStatusCode END
			FROM Project.tbProject Project JOIN Project_flow ON Project_flow.ChildProjectCode = Project.ProjectCode
			WHERE Project.ProjectStatusCode < 2;

			--not triggering fix
			WITH cascade_status AS
			(
				SELECT ProjectCode, ProjectStatusCode
				FROM Project.tbProject inserted
				WHERE NOT CashCode IS NULL AND ProjectStatusCode > 1
			), Project_flow AS
			(
				SELECT cascade_status.ProjectStatusCode ParentStatusCode, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectStatusCode
				FROM Project.tbFlow child 
					JOIN cascade_status ON child.ParentProjectCode = cascade_status.ProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
				WHERE CashCode IS NULL

				UNION ALL

				SELECT parent.ParentStatusCode, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectStatusCode
				FROM Project.tbFlow child 
					JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
				WHERE CashCode IS NULL
			)
			UPDATE ops
			SET OpStatusCode = 2
			FROM Project.tbOp ops JOIN Project_flow ON Project_flow.ChildProjectCode = ops.ProjectCode
			WHERE ops.OpStatusCode < 2;

			DELETE cost_set 
			FROM inserted 
				JOIN Project.tbCostSet cost_set ON inserted.ProjectCode = cost_set.ProjectCode
			WHERE inserted.ProjectStatusCode > 0;
			
		END

		IF UPDATE(Quantity)
		BEGIN
			UPDATE flow
			SET UsedOnQuantity = inserted.Quantity / parent_Project.Quantity
			FROM Project.tbFlow AS flow 
				JOIN inserted ON flow.ChildProjectCode = inserted.ProjectCode 
				JOIN Project.tbProject AS parent_Project ON flow.ParentProjectCode = parent_Project.ProjectCode
				JOIN Cash.tbCode ON parent_Project.CashCode = Cash.tbCode.CashCode
			WHERE (flow.UsedOnQuantity <> 0) AND (inserted.Quantity <> 0) 
				AND (inserted.Quantity / parent_Project.Quantity <> flow.UsedOnQuantity)
		END

		IF UPDATE(Quantity) OR UPDATE(UnitCharge)
		BEGIN
			UPDATE Project
			SET Project.TotalCharge = i.Quantity * i.UnitCharge
			FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode
		END

		IF UPDATE(TotalCharge)
		BEGIN
			UPDATE Project
			SET Project.UnitCharge = CASE i.TotalCharge + i.Quantity WHEN 0 THEN 0 ELSE i.TotalCharge / i.Quantity END
			FROM Project.tbProject Project JOIN inserted i ON Project.ProjectCode = i.ProjectCode			
		END

		IF UPDATE(ActionOn)
		BEGIN			
			WITH parent_Project AS
			(
				SELECT        ParentProjectCode
				FROM            Project.tbFlow flow
					JOIN Project.tbProject Project ON flow.ParentProjectCode = Project.ProjectCode
					JOIN Cash.tbCode cash ON Project.CashCode = cash.CashCode
					JOIN inserted ON flow.ChildProjectCode = inserted.ProjectCode
				--manual scheduling only
				WHERE (SELECT SUM(UsedOnQuantity) FROM inserted JOIN Project.tbFlow ON inserted.ProjectCode = Project.tbFlow.ChildProjectCode) = 0	
			), Project_flow AS
			(
				SELECT        flow.ParentProjectCode, flow.StepNumber, Project.ActionOn,
						LAG(Project.ActionOn, 1, Project.ActionOn) OVER (PARTITION BY flow.ParentProjectCode ORDER BY StepNumber) AS PrevActionOn
				FROM Project.tbFlow flow
					JOIN Project.tbProject Project ON flow.ChildProjectCode = Project.ProjectCode
					JOIN parent_Project ON flow.ParentProjectCode = parent_Project.ParentProjectCode
			), step_disordered AS
			(
				SELECT ParentProjectCode 
				FROM Project_flow
				WHERE ActionOn < PrevActionOn
				GROUP BY ParentProjectCode
			), step_ordered AS
			(
				SELECT flow.ParentProjectCode, flow.ChildProjectCode,
					ROW_NUMBER() OVER (PARTITION BY flow.ParentProjectCode ORDER BY Project.ActionOn, flow.StepNumber) * 10 AS StepNumber 
				FROM step_disordered
					JOIN Project.tbFlow flow ON step_disordered.ParentProjectCode = flow.ParentProjectCode
					JOIN Project.tbProject Project ON flow.ChildProjectCode = Project.ProjectCode
			)
			UPDATE flow
			SET
				StepNumber = step_ordered.StepNumber
			FROM Project.tbFlow flow
				JOIN step_ordered ON flow.ParentProjectCode = step_ordered.ParentProjectCode AND flow.ChildProjectCode = step_ordered.ChildProjectCode;
			
			IF EXISTS(SELECT * FROM App.tbOptions WHERE IsAutoOffsetDays <> 0)
			BEGIN
				UPDATE flow
				SET OffsetDays = App.fnOffsetDays(inserted.ActionOn, parent_Project.ActionOn)
									- ISNULL((SELECT SUM(OffsetDays) FROM Project.tbFlow sub_flow WHERE sub_flow.ParentProjectCode = flow.ParentProjectCode AND sub_flow.StepNumber > flow.StepNumber), 0)
				FROM Project.tbFlow AS flow 
					JOIN inserted ON flow.ChildProjectCode = inserted.ProjectCode 
					JOIN Project.tbProject AS parent_Project ON flow.ParentProjectCode = parent_Project.ProjectCode
					JOIN Cash.tbCode ON parent_Project.CashCode = Cash.tbCode.CashCode
				WHERE (SELECT SUM(UsedOnQuantity) FROM inserted JOIN Project.tbFlow ON inserted.ProjectCode = Project.tbFlow.ChildProjectCode) = 0
			END

			UPDATE Project
			SET PaymentOn = App.fnAdjustToCalendar(CASE WHEN Subject.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, i.ActionOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, Subject.PaymentDays + Subject.ExpectedDays, i.ActionOn)	
													END, 0) 
			FROM Project.tbProject Project
				JOIN inserted i ON Project.ProjectCode = i.ProjectCode
				JOIN Subject.tbSubject Subject ON i.SubjectCode = Subject.SubjectCode				
			WHERE NOT Project.CashCode IS NULL 
		END

		IF UPDATE (ProjectTitle)
		BEGIN
			WITH cascade_title_change AS
			(
				SELECT inserted.ProjectCode, inserted.ProjectTitle AS NewTitle, deleted.ProjectTitle AS PreviousTitle 				
				FROM inserted
					JOIN deleted ON inserted.ProjectCode = deleted.ProjectCode
			), Project_flow AS
			(
				SELECT cascade_title_change.NewTitle, cascade_title_change.PreviousTitle, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectTitle
				FROM Project.tbFlow child 
					JOIN cascade_title_change ON child.ParentProjectCode = cascade_title_change.ProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode

				UNION ALL

				SELECT parent.NewTitle, parent.PreviousTitle, child.ParentProjectCode, child.ChildProjectCode, child_Project.ProjectTitle
				FROM Project.tbFlow child 
					JOIN Project_flow parent ON child.ParentProjectCode = parent.ChildProjectCode
					JOIN Project.tbProject child_Project ON child.ChildProjectCode = child_Project.ProjectCode
			)
			UPDATE Project
			SET ProjectTitle = NewTitle
			FROM Project.tbProject Project JOIN Project_flow ON Project.ProjectCode = Project_flow.ChildProjectCode
			WHERE Project_flow.PreviousTitle = Project_flow.ProjectTitle;
		END

		IF UPDATE (Spooled)
		BEGIN
			INSERT INTO App.tbDocSpool (DocTypeCode, DocumentNumber)
			SELECT CASE 
					WHEN CashPolarityCode = 0 THEN		--Expense
						CASE WHEN ProjectStatusCode = 0 THEN 2	ELSE 3 END	--Enquiry								
					WHEN CashPolarityCode = 1 THEN		--Income
						CASE WHEN ProjectStatusCode = 0 THEN 0	ELSE 1 END	--Quote
					END AS DocTypeCode, Project.ProjectCode
			FROM   inserted Project INNER JOIN
									 Cash.tbCode ON Project.CashCode = Cash.tbCode.CashCode INNER JOIN
									 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE (Project.Spooled <> 0)
				
			DELETE App.tbDocSpool
			FROM         inserted i INNER JOIN
								  App.tbDocSpool ON i.ProjectCode = App.tbDocSpool.DocumentNumber
			WHERE    (i.Spooled = 0) AND ( App.tbDocSpool.DocTypeCode <= 3)
		END

		IF UPDATE (ContactName)
		BEGIN
			INSERT INTO Subject.tbContact (SubjectCode, ContactName)
			SELECT DISTINCT SubjectCode, ContactName FROM inserted
			WHERE EXISTS (SELECT     *
						FROM         inserted AS i
						WHERE     (NOT (ContactName IS NULL)) AND
												(ContactName <> N''))
				AND NOT EXISTS(SELECT  *
								FROM inserted AS i 
								INNER JOIN Subject.tbContact ON i.SubjectCode = Subject.tbContact.SubjectCode AND i.ContactName = Subject.tbContact.ContactName)
		END
		
		UPDATE Project.tbProject
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbProject INNER JOIN inserted AS i ON tbProject.ProjectCode = i.ProjectCode;

		IF UPDATE(ProjectStatusCode) OR UPDATE(Quantity) OR UPDATE(ActionOn) OR UPDATE(UnitCharge) OR UPDATE(ObjectCode) OR UPDATE(CashCode) OR UPDATE (TaxCode)
		BEGIN
			WITH candidates AS
			(
				SELECT ProjectCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge
				FROM inserted
				WHERE EXISTS (SELECT * FROM Project.tbChangeLog WHERE ProjectCode = inserted.ProjectCode)
			)
			, logs AS
			(
				SELECT clog.LogId, clog.ProjectCode, clog.SubjectCode, clog.ObjectCode, clog.ProjectStatusCode, clog.TransmitStatusCode, clog.ActionOn, clog.Quantity, clog.CashCode, clog.TaxCode, clog.UnitCharge
				FROM Project.tbChangeLog clog
				JOIN candidates ON clog.ProjectCode = candidates.ProjectCode AND LogId = (SELECT MAX(LogId) FROM Project.tbChangeLog WHERE ProjectCode = candidates.ProjectCode)		
			)
			INSERT INTO Project.tbChangeLog
									(ProjectCode, TransmitStatusCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
			SELECT candidates.ProjectCode, CASE Subjects.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, candidates.SubjectCode,
				candidates.ObjectCode, candidates.ProjectStatusCode, candidates.ActionOn, candidates.Quantity, candidates.CashCode, candidates.TaxCode, candidates.UnitCharge
			FROM candidates 
				JOIN Subject.tbSubject Subjects ON candidates.SubjectCode = Subjects.SubjectCode 
				JOIN logs ON candidates.ProjectCode = logs.ProjectCode
			WHERE (logs.ProjectStatusCode <> candidates.ProjectStatusCode) 
				OR (logs.TransmitStatusCode < 2)
				OR (logs.ActionOn <> candidates.ActionOn) 
				OR (logs.Quantity <> candidates.Quantity)
				OR (logs.UnitCharge <> candidates.UnitCharge)
				OR (logs.TaxCode <> candidates.TaxCode);
		END;

		IF UPDATE(SubjectCode)
		BEGIN
			WITH candidates AS
			(
				SELECT inserted.* FROM inserted
				JOIN deleted ON inserted.ProjectCode = deleted.ProjectCode
				WHERE inserted.SubjectCode <> deleted.SubjectCode
			)
			INSERT INTO Project.tbChangeLog
									 (ProjectCode, TransmitStatusCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
			SELECT candidates.ProjectCode, Subject.tbSubject.TransmitStatusCode, candidates.SubjectCode, candidates.ObjectCode, candidates.ProjectStatusCode, 
									 candidates.ActionOn, candidates.Quantity, candidates.CashCode, candidates.TaxCode, candidates.UnitCharge
			FROM candidates 
				JOIN Subject.tbSubject ON candidates.SubjectCode = Subject.tbSubject.SubjectCode
				JOIN Cash.tbCode ON candidates.CashCode = Cash.tbCode.CashCode;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE   TRIGGER Project.Project_tbProject_TriggerDelete
ON Project.tbProject
FOR DELETE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		INSERT INTO Project.tbChangeLog
								 (ProjectCode, TransmitStatusCode, SubjectCode, ObjectCode, ProjectStatusCode, ActionOn, Quantity, CashCode, TaxCode, UnitCharge)
		SELECT deleted.ProjectCode, CASE Subject.tbSubject.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, 
					deleted.SubjectCode, deleted.ObjectCode, 4 CancelledStatusCode, 
					deleted.ActionOn, deleted.Quantity, deleted.CashCode, deleted.TaxCode, deleted.UnitCharge
		FROM deleted INNER JOIN Subject.tbSubject ON deleted.SubjectCode = Subject.tbSubject.SubjectCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
