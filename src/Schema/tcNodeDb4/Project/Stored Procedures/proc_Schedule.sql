
CREATE   PROCEDURE Project.proc_Schedule (@ParentProjectCode nvarchar(20))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION;

		WITH ops_top_level AS
		(
			SELECT Project.ProjectCode, ops.OperationNumber, ops.OffsetDays, Project.ActionOn, ops.StartOn, ops.EndOn, Project.ProjectStatusCode, ops.OpStatusCode, ops.SyncTypeCode
			FROM Project.tbOp ops JOIN Project.tbProject Project ON ops.ProjectCode = Project.ProjectCode
			WHERE Project.ProjectCode = @ParentProjectCode
		), ops_candidates AS
		(
			SELECT *,
				ROW_NUMBER() OVER (PARTITION BY ProjectCode ORDER BY ProjectCode, OperationNumber DESC) AS LastOpRow,
				ROW_NUMBER() OVER (PARTITION BY ProjectCode ORDER BY ProjectCode, OperationNumber) AS FirstOpRow
			FROM ops_top_level
		), ops_unscheduled1 AS
		(
			SELECT ProjectCode, OperationNumber,
				CASE ProjectStatusCode 
					WHEN 0 THEN 0 
					WHEN 1 THEN 
						CASE WHEN FirstOpRow = 1 AND OpStatusCode < 1 THEN 1 ELSE OpStatusCode END				
					ELSE 2
					END AS OpStatusCode,
				CASE WHEN LastOpRow = 1 THEN App.fnAdjustToCalendar(ActionOn, OffsetDays) ELSE StartOn END AS StartOn,
				CASE WHEN LastOpRow = 1 THEN ActionOn ELSE EndOn END AS EndOn,
				LastOpRow,
				OffsetDays,
				CASE SyncTypeCode WHEN 1 THEN 0 ELSE OffsetDays END AS AsyncOffsetDays
			FROM ops_candidates
		)
		, ops_unscheduled2 AS
		(
			SELECT ProjectCode, OperationNumber, OpStatusCode, 
				FIRST_VALUE(EndOn) OVER (PARTITION BY ProjectCode ORDER BY OperationNumber DESC) AS ActionOn, 
				LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY ProjectCode ORDER BY OperationNumber DESC) AS AsyncOffsetDays,
				OffsetDays
			FROM ops_unscheduled1
		), ops_scheduled AS
		(
			SELECT ProjectCode, OperationNumber, OpStatusCode,
				App.fnAdjustToCalendar(ActionOn, SUM(AsyncOffsetDays) OVER (PARTITION BY ProjectCode ORDER BY OperationNumber DESC)) AS EndOn,
				App.fnAdjustToCalendar(ActionOn, SUM(AsyncOffsetDays) OVER (PARTITION BY ProjectCode ORDER BY OperationNumber DESC) + OffsetDays) AS StartOn
			FROM ops_unscheduled2
		)
		UPDATE op
		SET OpStatusCode = ops_scheduled.OpStatusCode,
			StartOn = ops_scheduled.StartOn, EndOn = ops_scheduled.EndOn
		FROM Project.tbOp op JOIN ops_scheduled 
			ON op.ProjectCode = ops_scheduled.ProjectCode AND op.OperationNumber = ops_scheduled.OperationNumber;

		WITH first_op AS
		(
			SELECT Project.tbOp.ProjectCode, MIN(Project.tbOp.StartOn) EndOn
			FROM Project.tbOp
			WHERE  (Project.tbOp.ProjectCode = @ParentProjectCode)
			GROUP BY Project.tbOp.ProjectCode
		), parent_Project AS
		(
			SELECT  Project.tbProject.ProjectCode, ProjectStatusCode, Quantity, ISNULL(EndOn, Project.tbProject.ActionOn) AS EndOn, Project.tbProject.ActionOn
			FROM Project.tbProject LEFT OUTER JOIN first_op ON first_op.ProjectCode = Project.tbProject.ProjectCode
			WHERE  (Project.tbProject.ProjectCode = @ParentProjectCode)	
		), Project_flow AS
		(
			SELECT work_flow.ParentProjectCode, work_flow.ChildProjectCode, work_flow.StepNumber,
				CASE WHEN work_flow.UsedOnQuantity <> 0 THEN parent_Project.Quantity * work_flow.UsedOnQuantity ELSE child_Project.Quantity END AS Quantity, 
				CASE WHEN parent_Project.ProjectStatusCode < 3 AND child_Project.ProjectStatusCode < parent_Project.ProjectStatusCode 
					THEN parent_Project.ProjectStatusCode 
					ELSE child_Project.ProjectStatusCode 
					END AS ProjectStatusCode,
				CASE SyncTypeCode WHEN 2 THEN parent_Project.ActionOn ELSE parent_Project.EndOn END AS EndOn, 
				parent_Project.ActionOn,
				CASE SyncTypeCode WHEN 0 THEN 0 ELSE OffsetDays END  AS OffsetDays,
				CASE SyncTypeCode WHEN 1 THEN 0 ELSE OffsetDays END AS AsyncOffsetDays,
				SyncTypeCode
			FROM parent_Project 
				JOIN Project.tbFlow work_flow ON parent_Project.ProjectCode = work_flow.ParentProjectCode
				JOIN Project.tbProject child_Project ON work_flow.ChildProjectCode = child_Project.ProjectCode
				
		), calloff_Projects_lag AS
		(
			SELECT ParentProjectCode, ChildProjectCode, StepNumber, Quantity, ProjectStatusCode, ActionOn EndOn, OffsetDays, 
					LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY ParentProjectCode ORDER BY StepNumber DESC) AS AsyncOffsetDays, 2SyncTypeCode	 
			FROM Project_flow
			WHERE EXISTS(SELECT * FROM Project_flow WHERE SyncTypeCode = 2)
				AND (StepNumber > (SELECT TOP 1 StepNumber FROM Project_flow WHERE SyncTypeCode = 0 ORDER BY StepNumber DESC)
					OR NOT EXISTS (SELECT * FROM Project_flow WHERE SyncTypeCode = 0))
		), calloff_Projects AS
		(
			SELECT ParentProjectCode, ChildProjectCode, StepNumber, Quantity, ProjectStatusCode, EndOn, OffsetDays, 
				SUM(AsyncOffsetDays) OVER (PARTITION BY ParentProjectCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM calloff_Projects_lag
		), servicing_Projects_lag AS
		(
			SELECT ParentProjectCode, ChildProjectCode, StepNumber, Quantity, ProjectStatusCode, EndOn, OffsetDays, 
					LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY ParentProjectCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM Project_flow
			WHERE (StepNumber < (SELECT MIN(StepNumber) FROM calloff_Projects_lag))
				OR NOT EXISTS (SELECT * FROM Project_flow WHERE SyncTypeCode = 2)
		), servicing_Projects AS
		(
			SELECT ParentProjectCode, ChildProjectCode, StepNumber, Quantity, ProjectStatusCode, EndOn, OffsetDays, 
				SUM(AsyncOffsetDays) OVER (PARTITION BY ParentProjectCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM servicing_Projects_lag
		), schedule AS
		(
			SELECT ChildProjectCode AS ProjectCode, Quantity, ProjectStatusCode, 
				DATEADD(DAY, (AsyncOffsetDays + OffsetDays) * -1, EndOn) AS ActionOn
			FROM calloff_Projects
			UNION
			SELECT ChildProjectCode AS ProjectCode, Quantity, ProjectStatusCode, 
				DATEADD(DAY, (AsyncOffsetDays + OffsetDays) * -1, EndOn) AS ActionOn
			FROM servicing_Projects
		)
		UPDATE Project
		SET
			Quantity = schedule.Quantity,
			ActionOn = schedule.ActionOn,
			ProjectStatusCode = schedule.ProjectStatusCode
		FROM Project.tbProject Project
			JOIN schedule ON Project.ProjectCode = schedule.ProjectCode;

		DECLARE child_Projects CURSOR LOCAL FOR
			SELECT ChildProjectCode FROM Project.tbFlow WHERE ParentProjectCode = @ParentProjectCode;

		DECLARE @ChildProjectCode NVARCHAR(20);

		OPEN child_Projects;

		FETCH NEXT FROM child_Projects INTO @ChildProjectCode
		
		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			EXEC Project.proc_Schedule @ChildProjectCode
			FETCH NEXT FROM child_Projects INTO @ChildProjectCode
		END

		CLOSE child_Projects;
		DEALLOCATE child_Projects;

		IF @@NESTLEVEL = 1
			COMMIT TRANSACTION;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
