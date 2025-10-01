
CREATE   PROCEDURE Task.proc_Schedule (@ParentTaskCode nvarchar(20))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION;

		WITH ops_top_level AS
		(
			SELECT task.TaskCode, ops.OperationNumber, ops.OffsetDays, task.ActionOn, ops.StartOn, ops.EndOn, task.TaskStatusCode, ops.OpStatusCode, ops.SyncTypeCode
			FROM Task.tbOp ops JOIN Task.tbTask task ON ops.TaskCode = task.TaskCode
			WHERE task.TaskCode = @ParentTaskCode
		), ops_candidates AS
		(
			SELECT *,
				ROW_NUMBER() OVER (PARTITION BY TaskCode ORDER BY TaskCode, OperationNumber DESC) AS LastOpRow,
				ROW_NUMBER() OVER (PARTITION BY TaskCode ORDER BY TaskCode, OperationNumber) AS FirstOpRow
			FROM ops_top_level
		), ops_unscheduled1 AS
		(
			SELECT TaskCode, OperationNumber,
				CASE TaskStatusCode 
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
			SELECT TaskCode, OperationNumber, OpStatusCode, 
				FIRST_VALUE(EndOn) OVER (PARTITION BY TaskCode ORDER BY OperationNumber DESC) AS ActionOn, 
				LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY TaskCode ORDER BY OperationNumber DESC) AS AsyncOffsetDays,
				OffsetDays
			FROM ops_unscheduled1
		), ops_scheduled AS
		(
			SELECT TaskCode, OperationNumber, OpStatusCode,
				App.fnAdjustToCalendar(ActionOn, SUM(AsyncOffsetDays) OVER (PARTITION BY TaskCode ORDER BY OperationNumber DESC)) AS EndOn,
				App.fnAdjustToCalendar(ActionOn, SUM(AsyncOffsetDays) OVER (PARTITION BY TaskCode ORDER BY OperationNumber DESC) + OffsetDays) AS StartOn
			FROM ops_unscheduled2
		)
		UPDATE op
		SET OpStatusCode = ops_scheduled.OpStatusCode,
			StartOn = ops_scheduled.StartOn, EndOn = ops_scheduled.EndOn
		FROM Task.tbOp op JOIN ops_scheduled 
			ON op.TaskCode = ops_scheduled.TaskCode AND op.OperationNumber = ops_scheduled.OperationNumber;

		WITH first_op AS
		(
			SELECT Task.tbOp.TaskCode, MIN(Task.tbOp.StartOn) EndOn
			FROM Task.tbOp
			WHERE  (Task.tbOp.TaskCode = @ParentTaskCode)
			GROUP BY Task.tbOp.TaskCode
		), parent_task AS
		(
			SELECT  Task.tbTask.TaskCode, TaskStatusCode, Quantity, ISNULL(EndOn, Task.tbTask.ActionOn) AS EndOn, Task.tbTask.ActionOn
			FROM Task.tbTask LEFT OUTER JOIN first_op ON first_op.TaskCode = Task.tbTask.TaskCode
			WHERE  (Task.tbTask.TaskCode = @ParentTaskCode)	
		), task_flow AS
		(
			SELECT work_flow.ParentTaskCode, work_flow.ChildTaskCode, work_flow.StepNumber,
				CASE WHEN work_flow.UsedOnQuantity <> 0 THEN parent_task.Quantity * work_flow.UsedOnQuantity ELSE child_task.Quantity END AS Quantity, 
				CASE WHEN parent_task.TaskStatusCode < 3 AND child_task.TaskStatusCode < parent_task.TaskStatusCode 
					THEN parent_task.TaskStatusCode 
					ELSE child_task.TaskStatusCode 
					END AS TaskStatusCode,
				CASE SyncTypeCode WHEN 2 THEN parent_task.ActionOn ELSE parent_task.EndOn END AS EndOn, 
				parent_task.ActionOn,
				CASE SyncTypeCode WHEN 0 THEN 0 ELSE OffsetDays END  AS OffsetDays,
				CASE SyncTypeCode WHEN 1 THEN 0 ELSE OffsetDays END AS AsyncOffsetDays,
				SyncTypeCode
			FROM parent_task 
				JOIN Task.tbFlow work_flow ON parent_task.TaskCode = work_flow.ParentTaskCode
				JOIN Task.tbTask child_task ON work_flow.ChildTaskCode = child_task.TaskCode
				
		), calloff_tasks_lag AS
		(
			SELECT ParentTaskCode, ChildTaskCode, StepNumber, Quantity, TaskStatusCode, ActionOn EndOn, OffsetDays, 
					LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY ParentTaskCode ORDER BY StepNumber DESC) AS AsyncOffsetDays, 2SyncTypeCode	 
			FROM task_flow
			WHERE EXISTS(SELECT * FROM task_flow WHERE SyncTypeCode = 2)
				AND (StepNumber > (SELECT TOP 1 StepNumber FROM task_flow WHERE SyncTypeCode = 0 ORDER BY StepNumber DESC)
					OR NOT EXISTS (SELECT * FROM task_flow WHERE SyncTypeCode = 0))
		), calloff_tasks AS
		(
			SELECT ParentTaskCode, ChildTaskCode, StepNumber, Quantity, TaskStatusCode, EndOn, OffsetDays, 
				SUM(AsyncOffsetDays) OVER (PARTITION BY ParentTaskCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM calloff_tasks_lag
		), servicing_tasks_lag AS
		(
			SELECT ParentTaskCode, ChildTaskCode, StepNumber, Quantity, TaskStatusCode, EndOn, OffsetDays, 
					LAG(AsyncOffsetDays, 1, 0) OVER (PARTITION BY ParentTaskCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM task_flow
			WHERE (StepNumber < (SELECT MIN(StepNumber) FROM calloff_tasks_lag))
				OR NOT EXISTS (SELECT * FROM task_flow WHERE SyncTypeCode = 2)
		), servicing_tasks AS
		(
			SELECT ParentTaskCode, ChildTaskCode, StepNumber, Quantity, TaskStatusCode, EndOn, OffsetDays, 
				SUM(AsyncOffsetDays) OVER (PARTITION BY ParentTaskCode ORDER BY StepNumber DESC) AS AsyncOffsetDays
			FROM servicing_tasks_lag
		), schedule AS
		(
			SELECT ChildTaskCode AS TaskCode, Quantity, TaskStatusCode, 
				DATEADD(DAY, (AsyncOffsetDays + OffsetDays) * -1, EndOn) AS ActionOn
			FROM calloff_tasks
			UNION
			SELECT ChildTaskCode AS TaskCode, Quantity, TaskStatusCode, 
				DATEADD(DAY, (AsyncOffsetDays + OffsetDays) * -1, EndOn) AS ActionOn
			FROM servicing_tasks
		)
		UPDATE task
		SET
			Quantity = schedule.Quantity,
			ActionOn = schedule.ActionOn,
			TaskStatusCode = schedule.TaskStatusCode
		FROM Task.tbTask task
			JOIN schedule ON task.TaskCode = schedule.TaskCode;

		DECLARE child_tasks CURSOR LOCAL FOR
			SELECT ChildTaskCode FROM Task.tbFlow WHERE ParentTaskCode = @ParentTaskCode;

		DECLARE @ChildTaskCode NVARCHAR(20);

		OPEN child_tasks;

		FETCH NEXT FROM child_tasks INTO @ChildTaskCode
		
		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			EXEC Task.proc_Schedule @ChildTaskCode
			FETCH NEXT FROM child_tasks INTO @ChildTaskCode
		END

		CLOSE child_tasks;
		DEALLOCATE child_tasks;

		IF @@NESTLEVEL = 1
			COMMIT TRANSACTION;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
