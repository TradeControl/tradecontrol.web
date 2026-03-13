CREATE PROCEDURE App.proc_DatasetObjectPrint
(
	@ObjectCode NVARCHAR(50),
	@IncludeOps BIT = 0,
	@IncludeAttributes BIT = 0
)
AS
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRY

		IF @ObjectCode IS NULL OR LTRIM(RTRIM(@ObjectCode)) = N''
			THROW 51020, 'DatasetObjectPrint: @ObjectCode is required.', 1;

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @ObjectCode)
			THROW 51021, 'DatasetObjectPrint: @ObjectCode not found in Object.tbObject.', 1;

		DECLARE @Line NVARCHAR(4000);

		;WITH object_tree AS
		(
			SELECT
				0 AS [Level],
				CAST(@ObjectCode AS NVARCHAR(50)) AS ObjectCode,
				CAST(NULL AS NVARCHAR(50)) AS ParentCode,
				CAST(0 AS smallint) AS StepNumber,
				CAST(1.000000 AS decimal(18, 6)) AS UsedOnQuantity,
				CAST(N'0000' AS nvarchar(400)) AS NodePath
			UNION ALL
			SELECT
				parent.[Level] + 1 AS [Level],
				f.ChildCode AS ObjectCode,
				f.ParentCode AS ParentCode,
				f.StepNumber,
				f.UsedOnQuantity,
				CAST(CONCAT(parent.NodePath, N'.', FORMAT(f.StepNumber, '0000')) AS nvarchar(400)) AS NodePath
			FROM object_tree parent
				JOIN Object.tbFlow f
					ON parent.ObjectCode = f.ParentCode
		),
		nodes AS
		(
			SELECT
				t.[Level],
				t.ObjectCode,
				o.ObjectDescription,
				o.UnitOfMeasure,
				t.StepNumber,
				t.UsedOnQuantity,
				t.NodePath
			FROM object_tree t
				JOIN Object.tbObject o
					ON t.ObjectCode = o.ObjectCode
		),
		lines AS
		(
			-- Node header (must sort before attributes/ops and before any children)
			SELECT
				CAST(CONCAT(n.NodePath, N'.!.00.HD') AS nvarchar(400)) AS NodePath,
				n.[Level],
				n.ObjectCode,
				0 AS LineType,
				CAST(0 AS int) AS Sort2,
				CAST(0 AS int) AS Sort3,
				CONCAT
				(
					REPLICATE(N'-- ', n.[Level]),
					n.ObjectCode,
					N'  [Qty=', FORMAT(n.UsedOnQuantity, '0.######'),
					N' ', ISNULL(n.UnitOfMeasure, N''), N'] ',
					ISNULL(n.ObjectDescription, N'')
				) AS LineText
			FROM nodes n

			UNION ALL

			-- Attributes at this node (after header, before ops, before children)
			SELECT
				CAST
				(
					CONCAT
					(
						n.NodePath,
						N'.!.01.AT.',
						FORMAT(a.PrintOrder, '0000'),
						N'.',
						FORMAT(ROW_NUMBER() OVER (PARTITION BY a.ObjectCode, a.PrintOrder ORDER BY a.Attribute), '0000')
					) AS nvarchar(400)
				) AS NodePath,
				n.[Level],
				n.ObjectCode,
				1 AS LineType,
				a.PrintOrder AS Sort2,
				ROW_NUMBER() OVER (PARTITION BY a.ObjectCode, a.PrintOrder ORDER BY a.Attribute) AS Sort3,
				CONCAT
				(
					REPLICATE(N'-- ', n.[Level]),
					N'** ',
					FORMAT(a.PrintOrder, '000'),
					N' [', ISNULL(at.AttributeType, CONVERT(nvarchar(10), a.AttributeTypeCode)), N'] ',
					a.Attribute,
					N': ',
					CASE
						WHEN a.DefaultText IS NULL OR LTRIM(RTRIM(a.DefaultText)) = N'' THEN N''
						WHEN LEN(a.DefaultText) <= 60 THEN a.DefaultText
						ELSE CONCAT(LEFT(a.DefaultText, 60), N'...')
					END
				) AS LineText
			FROM nodes n
				JOIN Object.tbAttribute a
					ON n.ObjectCode = a.ObjectCode
				LEFT JOIN Object.tbAttributeType at
					ON a.AttributeTypeCode = at.AttributeTypeCode
			WHERE @IncludeAttributes <> 0

			UNION ALL

			-- Ops at this node (after header/attributes, before children)
			SELECT
				CAST(CONCAT(n.NodePath, N'.!.02.OP.', FORMAT(op.OperationNumber, '0000')) AS nvarchar(400)) AS NodePath,
				n.[Level],
				n.ObjectCode,
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
				JOIN Object.tbOp op
					ON n.ObjectCode = op.ObjectCode
				LEFT JOIN Object.tbSyncType st
					ON op.SyncTypeCode = st.SyncTypeCode
			WHERE @IncludeOps <> 0
		)
		SELECT
			LineText,
			NodePath,
			[Level],
			ObjectCode,
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
				ObjectCode;

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
