CREATE PROCEDURE App.proc_DatasetProjectPrint
(
	@ParentProjectCode nvarchar(20),
	@IncludeOps BIT = 0,
	@IncludeAttributes BIT = 0
)
AS
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY

		IF @ParentProjectCode IS NULL OR LTRIM(RTRIM(@ParentProjectCode)) = N''
			THROW 51020, 'DatasetProjectPrint: @ParentProjectCode is required.', 1;

		IF NOT EXISTS (SELECT 1 FROM Project.tbProject WHERE ProjectCode = @ParentProjectCode)
			THROW 51021, 'DatasetProjectPrint: @ParentProjectCode not found in Project.tbProject.', 1;

		DECLARE @Line nvarchar(4000);

		;WITH project_tree AS
		(
			SELECT
				0 AS [Level],
				CAST(@ParentProjectCode AS nvarchar(20)) AS ProjectCode,
				CAST(NULL AS nvarchar(20)) AS ParentProjectCode,
				CAST(0 AS smallint) AS StepNumber,
				CAST(1.000000 AS decimal(18, 6)) AS UsedOnQuantity,
				CAST(N'0000' AS nvarchar(400)) AS NodePath

			UNION ALL

			SELECT
				parent.[Level] + 1 AS [Level],
				f.ChildProjectCode AS ProjectCode,
				f.ParentProjectCode AS ParentProjectCode,
				f.StepNumber,
				f.UsedOnQuantity,
				CAST(CONCAT(parent.NodePath, N'.', FORMAT(f.StepNumber, '0000')) AS nvarchar(400)) AS NodePath
			FROM project_tree parent
				JOIN Project.tbFlow f
					ON parent.ProjectCode = f.ParentProjectCode
		),
		nodes AS
		(
			SELECT
				t.[Level],
				t.ProjectCode,
				p.ObjectCode,
				p.SubjectCode,
				s.SubjectName,
				p.ActionOn,
				p.Quantity,
				p.UnitCharge,
				p.TotalCharge,
				o.UnitOfMeasure,
				o.ObjectDescription,
				p.ProjectStatusCode,
				p.ProjectTitle,
				t.StepNumber,
				t.UsedOnQuantity,
				t.NodePath
			FROM project_tree t
				JOIN Project.tbProject p
					ON t.ProjectCode = p.ProjectCode
				LEFT JOIN Object.tbObject o
					ON p.ObjectCode = o.ObjectCode
				LEFT JOIN Subject.tbSubject s
					ON p.SubjectCode = s.SubjectCode
		),
		lines AS
		(
			-- Node header (must sort before attributes/ops and before any children)
			SELECT
				CAST(CONCAT(n.NodePath, N'.!.00.HD') AS nvarchar(400)) AS NodePath,
				n.[Level],
				n.ProjectCode,
				0 AS LineType,
				CAST(0 AS int) AS Sort2,
				CAST(0 AS int) AS Sort3,
				CONCAT
				(
					REPLICATE(N'-- ', n.[Level]),
					n.ProjectCode,
					N'  [Qty=', FORMAT(ISNULL(n.Quantity, 0), '0.######'),
					N' ', ISNULL(n.UnitOfMeasure, N''), N'] ',
					ISNULL(n.ObjectCode, N''),
					CASE WHEN ISNULL(n.ObjectDescription, N'') = N'' THEN N'' ELSE CONCAT(N'  ', n.ObjectDescription) END,
					N'  [Sub=', ISNULL(n.SubjectCode, N''), N' ', ISNULL(n.SubjectName, N''), N']',
					N'  [Act=', ISNULL(CONVERT(nvarchar(10), CAST(n.ActionOn AS date), 120), N''), N']',
					N'  [Unit=', FORMAT(ISNULL(n.UnitCharge, 0), '0.####'), N']',
					N'  [Charge=', FORMAT(ISNULL(n.TotalCharge, 0), '0.00'), N']'
				) AS LineText
			FROM nodes n

			UNION ALL

			-- Attributes on this node (Project.tbAttribute)
			SELECT
				CAST
				(
					CONCAT
					(
						n.NodePath,
						N'.!.01.AT.',
						FORMAT(a.PrintOrder, '0000'),
						N'.',
						FORMAT(ROW_NUMBER() OVER (PARTITION BY a.ProjectCode, a.PrintOrder ORDER BY a.Attribute), '0000')
					) AS nvarchar(400)
				) AS NodePath,
				n.[Level],
				n.ProjectCode,
				1 AS LineType,
				a.PrintOrder AS Sort2,
				ROW_NUMBER() OVER (PARTITION BY a.ProjectCode, a.PrintOrder ORDER BY a.Attribute) AS Sort3,
				CONCAT
				(
					REPLICATE(N'-- ', n.[Level]),
					N'** ',
					FORMAT(a.PrintOrder, '000'),
					N' [', ISNULL(at.AttributeType, CONVERT(nvarchar(10), a.AttributeTypeCode)), N'] ',
					a.Attribute,
					N': ',
					CASE
						WHEN a.AttributeDescription IS NULL OR LTRIM(RTRIM(a.AttributeDescription)) = N'' THEN N''
						WHEN LEN(a.AttributeDescription) <= 60 THEN a.AttributeDescription
						ELSE CONCAT(LEFT(a.AttributeDescription, 60), N'...')
					END
				) AS LineText
			FROM nodes n
				JOIN Project.tbAttribute a
					ON n.ProjectCode = a.ProjectCode
				LEFT JOIN Object.tbAttributeType at
					ON a.AttributeTypeCode = at.AttributeTypeCode
			WHERE @IncludeAttributes <> 0

			UNION ALL

			-- Ops on this node (Project.tbOp)
			SELECT
				CAST(CONCAT(n.NodePath, N'.!.02.OP.', FORMAT(op.OperationNumber, '0000')) AS nvarchar(400)) AS NodePath,
				n.[Level],
				n.ProjectCode,
				2 AS LineType,
				op.OperationNumber AS Sort2,
				CAST(0 AS int) AS Sort3,
				CONCAT
				(
					REPLICATE(N'-- ', n.[Level]),
					N'>> ',
					FORMAT(op.OperationNumber, '000'),
					N' [', ISNULL(st.SyncType, CONVERT(nvarchar(10), op.SyncTypeCode)), N'] ',
					op.Operation,
					N'  (Dur=', FORMAT(ISNULL(op.Duration, 0), '0.####'),
					N', Off=', CONVERT(nvarchar(10), ISNULL(op.OffsetDays, 0)),
					N')'
				) AS LineText
			FROM nodes n
				JOIN Project.tbOp op
					ON n.ProjectCode = op.ProjectCode
				LEFT JOIN Object.tbSyncType st
					ON op.SyncTypeCode = st.SyncTypeCode
			WHERE @IncludeOps <> 0
		)
		SELECT
			LineText,
			NodePath,
			[Level],
			ProjectCode,
			LineType,
			Sort2,
			Sort3
		INTO #print
		FROM lines;

		DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
			SELECT LineText
			FROM #print
			ORDER BY
				NodePath,
				LineType,
				Sort2,
				Sort3,
				ProjectCode;

		OPEN cur;
		FETCH NEXT FROM cur INTO @Line;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			PRINT @Line;
			FETCH NEXT FROM cur INTO @Line;
		END

		CLOSE cur;
		DEALLOCATE cur;

		DROP TABLE #print;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
GO
