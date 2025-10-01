CREATE   PROCEDURE Project.proc_Configure (@ParentProjectCode nvarchar(20))
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@StepNumber smallint
			, @ProjectCode nvarchar(20)
			, @UserId nvarchar(10)
			, @ObjectCode nvarchar(50)
			, @SubjectCode nvarchar(10)
			, @DefaultSubjectCode nvarchar(10)
			, @TaxCode nvarchar(10)

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION

		INSERT INTO Subject.tbContact 
			(SubjectCode, ContactName, FileAs, PhoneNumber, EmailAddress)
		SELECT Project.tbProject.SubjectCode, Project.tbProject.ContactName, Project.tbProject.ContactName AS NickName, Subject.tbSubject.PhoneNumber, Subject.tbSubject.EmailAddress
		FROM  Project.tbProject 
			INNER JOIN Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode
		WHERE LEN(ISNULL(Project.tbProject.ContactName, '')) > 0 AND (Project.tbProject.ProjectCode = @ParentProjectCode)
					AND EXISTS (SELECT *
								FROM Project.tbProject
								WHERE (ProjectCode = @ParentProjectCode) AND (NOT (ContactName IS NULL)) OR (ProjectCode = @ParentProjectCode) AND (ContactName <> N''))
				AND NOT EXISTS(SELECT *
								FROM  Project.tbProject 
									INNER JOIN Subject.tbContact ON Project.tbProject.SubjectCode = Subject.tbContact.SubjectCode AND Project.tbProject.ContactName = Subject.tbContact.ContactName
								WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode))
	
		UPDATE Subject.tbSubject
		SET SubjectStatusCode = 1
		FROM Subject.tbSubject INNER JOIN Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode
		WHERE ( Project.tbProject.ProjectCode = @ParentProjectCode) AND ( Subject.tbSubject.SubjectStatusCode = 0)				
			AND EXISTS(SELECT *
				FROM  Subject.tbSubject INNER JOIN Project.tbProject ON Subject.tbSubject.SubjectCode = Project.tbProject.SubjectCode
				WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode) AND ( Subject.tbSubject.SubjectStatusCode = 0))
	          
		UPDATE    Project.tbProject
		SET  ActionedOn = ActionOn
		WHERE (ProjectCode = @ParentProjectCode)
			AND EXISTS(SELECT *
					  FROM Project.tbProject
					  WHERE (ProjectStatusCode = 2) AND (ProjectCode = @ParentProjectCode))

		UPDATE Project.tbProject
		SET ProjectTitle = ObjectCode
		WHERE (ProjectCode = @ParentProjectCode)
			AND EXISTS(SELECT *
				  FROM Project.tbProject
				  WHERE (ProjectCode = @ParentProjectCode) AND (ProjectTitle IS NULL))  	 				              
	     	
		INSERT INTO Project.tbAttribute
			(ProjectCode, Attribute, AttributeDescription, PrintOrder, AttributeTypeCode)
		SELECT Project.tbProject.ProjectCode, Object.tbAttribute.Attribute, Object.tbAttribute.DefaultText, Object.tbAttribute.PrintOrder, Object.tbAttribute.AttributeTypeCode
		FROM Object.tbAttribute 
			INNER JOIN Project.tbProject ON Object.tbAttribute.ObjectCode = Project.tbProject.ObjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode)
	
		INSERT INTO Project.tbOp
			(ProjectCode, UserId, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays, StartOn)
		SELECT Project.tbProject.ProjectCode, Project.tbProject.UserId, Object.tbOp.OperationNumber, Object.tbOp.SyncTypeCode, Object.tbOp.Operation, Object.tbOp.Duration,  Object.tbOp.OffsetDays, Project.tbProject.ActionOn
		FROM Object.tbOp INNER JOIN Project.tbProject ON Object.tbOp.ObjectCode = Project.tbProject.ObjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode)
	                   
	
		SELECT @UserId = UserId FROM Project.tbProject WHERE Project.tbProject.ProjectCode = @ParentProjectCode
	
		DECLARE curAct cursor local for
			SELECT Object.tbFlow.StepNumber
			FROM Object.tbFlow INNER JOIN Project.tbProject ON Object.tbFlow.ParentCode = Project.tbProject.ObjectCode
			WHERE     ( Project.tbProject.ProjectCode = @ParentProjectCode)
			ORDER BY Object.tbFlow.StepNumber	
	
		OPEN curAct
		FETCH NEXT FROM curAct INTO @StepNumber
		WHILE @@FETCH_STATUS = 0
			BEGIN
			SELECT  
				@ObjectCode = Object.tbObject.ObjectCode, 
				@SubjectCode = Project.tbProject.SubjectCode
			FROM Object.tbFlow 
				INNER JOIN Object.tbObject ON Object.tbFlow.ChildCode = Object.tbObject.ObjectCode 
				INNER JOIN Project.tbProject ON Object.tbFlow.ParentCode = Project.tbProject.ObjectCode
			WHERE     ( Object.tbFlow.StepNumber = @StepNumber) AND ( Project.tbProject.ProjectCode = @ParentProjectCode)
		
			EXEC Project.proc_NextCode @ObjectCode, @ProjectCode output

			INSERT INTO Project.tbProject
				(ProjectCode, UserId, SubjectCode, ContactName, ObjectCode, ProjectStatusCode, ActionById, ActionOn, ProjectNotes, Quantity, UnitCharge, AddressCodeFrom, AddressCodeTo, CashCode, Printed, ProjectTitle)
			SELECT  @ProjectCode AS NewProject, Project_tb1.UserId, Project_tb1.SubjectCode, Project_tb1.ContactName, Object.tbObject.ObjectCode, Object.tbObject.ProjectStatusCode, 
						Project_tb1.ActionById, Project_tb1.ActionOn, Object.tbObject.ObjectDescription, Project_tb1.Quantity * Object.tbFlow.UsedOnQuantity AS Quantity,
						Object.tbObject.UnitCharge, Subject.tbSubject.AddressCode AS AddressCodeFrom, Subject.tbSubject.AddressCode AS AddressCodeTo, 
						tbObject.CashCode, CASE WHEN Object.tbObject.Printed = 0 THEN 1 ELSE 0 END AS Printed, Project_tb1.ProjectTitle
			FROM  Object.tbFlow 
				INNER JOIN Object.tbObject ON Object.tbFlow.ChildCode = Object.tbObject.ObjectCode 
				INNER JOIN Project.tbProject Project_tb1 ON Object.tbFlow.ParentCode = Project_tb1.ObjectCode 
				INNER JOIN Subject.tbSubject ON Project_tb1.SubjectCode = Subject.tbSubject.SubjectCode
			WHERE     ( Object.tbFlow.StepNumber = @StepNumber) AND ( Project_tb1.ProjectCode = @ParentProjectCode)

			IF EXISTS (SELECT * FROM Project.tbProject 
							INNER JOIN  Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode 
							INNER JOIN App.tbTaxCode ON Subject.tbSubject.TaxCode = App.tbTaxCode.TaxCode AND Subject.tbSubject.TaxCode = App.tbTaxCode.TaxCode)
				BEGIN
				UPDATE Project.tbProject
				SET TaxCode = App.tbTaxCode.TaxCode
				FROM Project.tbProject 
					INNER JOIN Subject.tbSubject ON Project.tbProject.SubjectCode = Subject.tbSubject.SubjectCode 
					INNER JOIN App.tbTaxCode ON Subject.tbSubject.TaxCode = App.tbTaxCode.TaxCode AND Subject.tbSubject.TaxCode = App.tbTaxCode.TaxCode
				WHERE (Project.tbProject.ProjectCode = @ProjectCode)
				END
			ELSE
				BEGIN
				UPDATE Project.tbProject
				SET TaxCode = Cash.tbCode.TaxCode
				FROM  Project.tbProject 
					INNER JOIN Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode
				WHERE  (Project.tbProject.ProjectCode = @ProjectCode)
				END			
			
			SELECT @DefaultSubjectCode = (SELECT TOP 1  SubjectCode FROM Project.tbProject
											WHERE   (ObjectCode = (SELECT ObjectCode FROM  Project.tbProject AS tbProject_1 WHERE (ProjectCode = @ProjectCode))) AND (ProjectCode <> @ProjectCode))

			IF NOT @DefaultSubjectCode IS NULL
				BEGIN
				UPDATE Project.tbProject
				SET SubjectCode = @DefaultSubjectCode
				WHERE (ProjectCode = @ProjectCode)
				END
					
			INSERT INTO Project.tbFlow
				(ParentProjectCode, StepNumber, ChildProjectCode, SyncTypeCode, UsedOnQuantity, OffsetDays)
			SELECT Project.tbProject.ProjectCode, Object.tbFlow.StepNumber, @ProjectCode AS ChildProjectCode, Object.tbFlow.SyncTypeCode, Object.tbFlow.UsedOnQuantity, Object.tbFlow.OffsetDays
			FROM Object.tbFlow 
				INNER JOIN Project.tbProject ON Object.tbFlow.ParentCode = Project.tbProject.ObjectCode
			WHERE (Project.tbProject.ProjectCode = @ParentProjectCode) AND ( Object.tbFlow.StepNumber = @StepNumber)
		
			EXEC Project.proc_Configure @ProjectCode

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
