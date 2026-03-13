CREATE PROCEDURE [App].[proc_DatasetCreateService]
(
	@ServiceName NVARCHAR(50) = N'Book',
	@UnitCharge DECIMAL(18, 7) = 0.0000000,
	@ObjectCode NVARCHAR(50) OUTPUT
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRAN;

		IF NOT EXISTS (SELECT 1 FROM Usr.vwCredentials WHERE IsAdministrator <> 0)
		BEGIN
			DECLARE @Msg NVARCHAR(100) = CONCAT('Access Denied: User ', SUSER_SNAME(), ' is not an administrsator');
			RAISERROR ('%s', 13, 1, @Msg);
		END

		DECLARE
			@ServiceNameNorm NVARCHAR(50) = LTRIM(RTRIM(COALESCE(@ServiceName, N''))),
			@SoObjectCode NVARCHAR(50),
			@PoObjectCode NVARCHAR(50),
			@PoTransportCode NVARCHAR(50) = N'PO Transport',
			@Uom NVARCHAR(15) = N'copies';

		IF @ServiceNameNorm = N''
			THROW 51030, 'proc_DatasetCreateService: @ServiceName is required.', 1;

		SET @SoObjectCode = CONCAT(N'SO ', @ServiceNameNorm);
		SET @PoObjectCode = CONCAT(N'PO ', @ServiceNameNorm);

		IF NOT EXISTS (SELECT 1 FROM App.tbUom WHERE UnitOfMeasure = @Uom)
			INSERT INTO App.tbUom (UnitOfMeasure) VALUES (@Uom);

		-- Output is always the root sales order
		SET @ObjectCode = @SoObjectCode;

		DECLARE
			@SoCashCode NVARCHAR(50) = N'TC100',
			@PoCashCode NVARCHAR(50) = N'TC200',
			@SoUnitCharge DECIMAL(18, 7) = ISNULL(@UnitCharge, 0.0000000),
			@PoUnitCharge DECIMAL(18, 7) = CAST(ROUND(ISNULL(@UnitCharge, 0.0000000) * 0.7, 2) AS DECIMAL(18, 7));

		---------------------------------------------------------------------
		-- Objects
		---------------------------------------------------------------------
		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @SoObjectCode)
		BEGIN
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@SoObjectCode, 1, N'', @Uom, @SoCashCode, @SoUnitCharge, 1, N'Sales Order');
		END

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @PoObjectCode)
		BEGIN
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@PoObjectCode, 1, N'', @Uom, @PoCashCode, @PoUnitCharge, 1, N'Purchase Order');
		END

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @PoTransportCode)
		BEGIN
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@PoTransportCode, 1, N'', N'each', @PoCashCode, CAST(100.0000000 AS decimal(18,7)), 1, N'Purchase Order');
		END

		---------------------------------------------------------------------
		-- Attributes (idempotent, per attribute)
		---------------------------------------------------------------------
		DECLARE @Attr TABLE
		(
			ObjectCode NVARCHAR(50) NOT NULL,
			Attribute NVARCHAR(50) NOT NULL,
			PrintOrder SMALLINT NOT NULL,
			AttributeTypeCode SMALLINT NOT NULL,
			DefaultText NVARCHAR(400) NULL
		);

		-- Template: SO Brochure or Catalogue -> SO <ServiceName>
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@SoObjectCode, N'Delivery #1', 160, 0, N''),
			(@SoObjectCode, N'Finishing', 90, 0, N''),
			(@SoObjectCode, N'Note', 150, 0, N''),
			(@SoObjectCode, N'Origination', 40, 0, N''),
			(@SoObjectCode, N'Packing', 100, 0, N''),
			(@SoObjectCode, N'Pagination', 20, 0, N''),
			(@SoObjectCode, N'Paper', 80, 0, N''),
			(@SoObjectCode, N'Printing', 60, 0, N''),
			(@SoObjectCode, N'Proofing', 50, 0, N''),
			(@SoObjectCode, N'Trim Size', 30, 0, N''),
			(@SoObjectCode, N'UV Varnish', 70, 0, N'');

		-- Template: PO Brochure or Catalogue -> PO <ServiceName>
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@PoObjectCode, N'Delivery #1', 155, 0, N''),
			(@PoObjectCode, N'File Copies', 160, 0, N''),
			(@PoObjectCode, N'Finishing', 90, 0, N''),
			(@PoObjectCode, N'Note', 150, 0, N''),
			(@PoObjectCode, N'Origination', 40, 0, N''),
			(@PoObjectCode, N'Packing', 100, 0, N''),
			(@PoObjectCode, N'Pagination', 20, 0, N''),
			(@PoObjectCode, N'Paper', 80, 0, N''),
			(@PoObjectCode, N'Printing', 60, 0, N''),
			(@PoObjectCode, N'Proofing', 50, 0, N''),
			(@PoObjectCode, N'Trim Size', 30, 0, N''),
			(@PoObjectCode, N'UV Varnish', 70, 0, N'');

		-- Template: PO Transport -> PO Transport
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@PoTransportCode, N'Collection', 20, 0, N''),
			(@PoTransportCode, N'Description', 10, 0, N''),
			(@PoTransportCode, N'Note', 30, 1, N'');

		MERGE Object.tbAttribute AS t
		USING @Attr AS s
			ON t.ObjectCode = s.ObjectCode
			AND t.Attribute = s.Attribute
		WHEN NOT MATCHED THEN
			INSERT (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
			VALUES (s.ObjectCode, s.Attribute, s.PrintOrder, s.AttributeTypeCode, s.DefaultText)
		WHEN MATCHED THEN
			UPDATE SET
				PrintOrder = s.PrintOrder,
				AttributeTypeCode = s.AttributeTypeCode,
				DefaultText = s.DefaultText;

		---------------------------------------------------------------------
		-- Operations (idempotent, per operation number)
		---------------------------------------------------------------------
		DECLARE @Ops TABLE
		(
			ObjectCode NVARCHAR(50) NOT NULL,
			OperationNumber SMALLINT NOT NULL,
			SyncTypeCode SMALLINT NOT NULL,
			Operation NVARCHAR(50) NOT NULL,
			Duration DECIMAL(18, 4) NULL,
			OffsetDays SMALLINT NOT NULL
		);

		-- Template: SO <ServiceName>
		INSERT INTO @Ops (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
		VALUES
			(@SoObjectCode, 10, 0, N'Artwork', 0, 0),
			(@SoObjectCode, 20, 0, N'Proofs', 0, 2),
			(@SoObjectCode, 30, 0, N'Approval', 0, 3),
			(@SoObjectCode, 40, 2, N'Delivery', 0, 5);

		-- Template: PO <ServiceName>
		INSERT INTO @Ops (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
		VALUES
			(@PoObjectCode, 10, 0, N'Artwork', 0, 0),
			(@PoObjectCode, 20, 0, N'Proofs', 0, 0),
			(@PoObjectCode, 30, 0, N'Approval', 0, 0),
			(@PoObjectCode, 50, 2, N'Delivery', 0, 0);

		-- Template: PO Transport
		INSERT INTO @Ops (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
		VALUES
			(@PoTransportCode, 10, 0, N'Despatch', 0, 0),
			(@PoTransportCode, 20, 2, N'Delivery', 0, 0);

		MERGE Object.tbOp AS t
		USING @Ops AS s
			ON t.ObjectCode = s.ObjectCode
			AND t.OperationNumber = s.OperationNumber
		WHEN NOT MATCHED THEN
			INSERT (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
			VALUES (s.ObjectCode, s.OperationNumber, s.SyncTypeCode, s.Operation, s.Duration, s.OffsetDays)
		WHEN MATCHED THEN
			UPDATE SET
				SyncTypeCode = s.SyncTypeCode,
				Operation = s.Operation,
				Duration = s.Duration,
				OffsetDays = s.OffsetDays;

		---------------------------------------------------------------------
		-- Flow (idempotent, per step)
		---------------------------------------------------------------------
		MERGE Object.tbFlow AS t
		USING
		(
			SELECT @SoObjectCode AS ParentCode, CAST(10 AS SMALLINT) AS StepNumber, @PoObjectCode AS ChildCode, CAST(0 AS SMALLINT) AS SyncTypeCode, CAST(0 AS SMALLINT) AS OffsetDays, CAST(1.000000 AS DECIMAL(18, 6)) AS UsedOnQuantity
			UNION ALL
			SELECT @SoObjectCode AS ParentCode, CAST(20 AS SMALLINT) AS StepNumber, @PoTransportCode AS ChildCode, CAST(0 AS SMALLINT) AS SyncTypeCode, CAST(0 AS SMALLINT) AS OffsetDays, CAST(0.000000 AS DECIMAL(18, 6)) AS UsedOnQuantity
		) AS s
			ON t.ParentCode = s.ParentCode
			AND t.StepNumber = s.StepNumber
		WHEN NOT MATCHED THEN
			INSERT (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (s.ParentCode, s.StepNumber, s.ChildCode, s.SyncTypeCode, s.OffsetDays, s.UsedOnQuantity)
		WHEN MATCHED THEN
			UPDATE SET
				ChildCode = s.ChildCode,
				SyncTypeCode = s.SyncTypeCode,
				OffsetDays = s.OffsetDays,
				UsedOnQuantity = s.UsedOnQuantity;

		COMMIT TRAN;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
GO
