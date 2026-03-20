CREATE PROCEDURE App.proc_DatasetCreateProduct
(
	@MaterialType NVARCHAR(20) = N'CLEAR',
	@ObjectCode NVARCHAR(50) OUTPUT
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF NOT EXISTS (SELECT 1 FROM Usr.vwCredentials WHERE IsAdministrator <> 0)
		BEGIN
			DECLARE @Msg NVARCHAR(100) = CONCAT('Access Denied: User ', SUSER_SNAME(), ' is not an administrsator');
			RAISERROR ('%s', 13, 1, @Msg);
		END

		DECLARE @ExchangeRate float =
			CASE (SELECT UnitOfCharge FROM App.tbOptions)
				WHEN 'BTC' THEN 0.135
				ELSE 1
			END;

		DECLARE
			@MaterialTypeNorm NVARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@MaterialType, N'')))),
			@ObjectPropertyCode NVARCHAR(2);

		IF @MaterialTypeNorm = N''
			SET @MaterialTypeNorm = N'CLEAR';

		IF @MaterialTypeNorm NOT IN (N'CLEAR', N'RED', N'BLUE', N'GREEN', N'BLACK', N'WHITE')
			THROW 51010, 'Dataset: invalid @MaterialType. Allowed: CLEAR, RED, BLUE, GREEN, BLACK, WHITE.', 1;

		SET @ObjectPropertyCode =
			CASE @MaterialTypeNorm
				WHEN N'RED' THEN N'00'
				WHEN N'BLUE' THEN N'01'
				WHEN N'GREEN' THEN N'02'
				WHEN N'BLACK' THEN N'03'
				WHEN N'WHITE' THEN N'04'
				WHEN N'CLEAR' THEN N'05'
			END;

		DECLARE
			@FgCode NVARCHAR(50) = CONCAT(N'DS/PRD/FG/W/', @ObjectPropertyCode),
			@SubShelfCode NVARCHAR(50) = CONCAT(N'DS/PRD/CMP/W/100/', @ObjectPropertyCode),
			@BackDividerCode NVARCHAR(50) = CONCAT(N'DS/PRD/CMP/W/101/', @ObjectPropertyCode),
			@WideFootCode NVARCHAR(50) = CONCAT(N'DS/PRD/CMP/W/097/', @ObjectPropertyCode),
			@NarrowFootCode NVARCHAR(50) = CONCAT(N'DS/PRD/CMP/W/099/', @ObjectPropertyCode),

			@PalletCode NVARCHAR(50) = CONCAT(N'DS/PRD/CMP/BOX/PALLET/', @ObjectPropertyCode),
			@Box41Code NVARCHAR(50) = CONCAT(N'DS/PRD/CMP/BOX/41/', @ObjectPropertyCode),
			@Box99Code NVARCHAR(50) = CONCAT(N'DS/PRD/CMP/BOX/99/', @ObjectPropertyCode),
			@Insert09Code NVARCHAR(50) = CONCAT(N'DS/PRD/CMP/INS/09/', @ObjectPropertyCode),

			@MaterialPcCode NVARCHAR(50) = CONCAT(N'DS/PRD/MTR/PC/', @ObjectPropertyCode),
			@DeliveryCode NVARCHAR(50) = CONCAT(N'DS/SRV/SHP/DELIVERY/', @ObjectPropertyCode);

		SET @ObjectCode = @FgCode;

		IF NOT EXISTS (SELECT 1 FROM App.tbRegister WHERE RegisterName = 'Works Order')
		BEGIN
			INSERT INTO App.tbRegister (RegisterName, NextNumber)
			SELECT 'Works Order', (SELECT MAX(NextNumber) + 10000 FROM App.tbRegister) AS NextNumber;
		END

		---------------------------------------------------------------------
		-- Objects (mirror proc_DemoBom "shape": UOM, unit charges, printed, registers)
		---------------------------------------------------------------------
		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @FgCode)
		BEGIN
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@FgCode, 1, CONCAT(N'DATASET WIDGET ASSEMBLY ', @MaterialTypeNorm), N'each', N'TC100', CAST(1.67 * @ExchangeRate AS decimal(18,7)), 1, N'Sales Order');
		END

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @SubShelfCode)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@SubShelfCode, 1, CONCAT(N'DATASET SUB SHELF ', @MaterialTypeNorm), N'each', NULL, 0, 0, N'Works Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @BackDividerCode)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@BackDividerCode, 1, N'DATASET BACK DIVIDER', N'each', NULL, 0, 0, N'Works Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @WideFootCode)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@WideFootCode, 1, N'DATASET DIVIDER (WIDE FOOT)', N'each', NULL, 0, 0, N'Works Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @NarrowFootCode)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@NarrowFootCode, 1, N'DATASET DIVIDER (NARROW FOOT)', N'each', NULL, 0, 0, N'Works Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @PalletCode)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@PalletCode, 1, N'DATASET EURO PALLET', N'each', N'TC200', CAST(2.4 * @ExchangeRate AS decimal(18,7)), 1, N'Purchase Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @Box41Code)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@Box41Code, 1, N'DATASET OUTER BOX', N'each', N'TC200', CAST(0.05 * @ExchangeRate AS decimal(18,7)), 1, N'Purchase Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @Box99Code)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@Box99Code, 1, N'DATASET INTERNAL BOX', N'each', NULL, 0, 0, N'Works Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @MaterialPcCode)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@MaterialPcCode, 1, CONCAT(N'DATASET PLASTIC ', @MaterialTypeNorm), N'kilo', N'TC200', CAST(0.22 * @ExchangeRate AS decimal(18,7)), 1, N'Purchase Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @Insert09Code)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@Insert09Code, 1, N'DATASET INSERTS', N'each', N'TC200', CAST(0.005 * @ExchangeRate AS decimal(18,7)), 1, N'Purchase Order');

		IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = @DeliveryCode)
			INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
			VALUES (@DeliveryCode, 1, N'DATASET DELIVERY', N'each', N'TC200', CAST(100.0000000 * @ExchangeRate AS decimal(18,7)), 1, N'Purchase Order');

		---------------------------------------------------------------------
		-- Attributes (mirror proc_DemoBom set; inject @MaterialTypeNorm for Colour)
		---------------------------------------------------------------------
		DECLARE @Attr AS TABLE
		(
			ObjectCode NVARCHAR(50) NOT NULL,
			Attribute NVARCHAR(50) NOT NULL,
			PrintOrder SMALLINT NOT NULL,
			AttributeTypeCode SMALLINT NOT NULL,
			DefaultText NVARCHAR(400) NULL
		);

		-- Finished good (equivalent to M/00/70/00)
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@FgCode, N'Colour', 20, 0, @MaterialTypeNorm),
			(@FgCode, N'Colour Number', 10, 0, N'-'),
			(@FgCode, N'Count Type', 50, 0, N'Weigh Count'),
			(@FgCode, N'Drawing Issue', 40, 0, N'1'),
			(@FgCode, N'Drawing Number', 30, 0, N'321554'),
			(@FgCode, N'Label Type', 70, 0, N'Assembly Card'),
			(@FgCode, N'Mould Tool Specification', 110, 1, NULL),
			(@FgCode, N'Pack Type', 60, 0, N'Despatched'),
			(@FgCode, N'Quantity/Box', 80, 0, N'100');

		-- Sub shelf (equivalent to M/100/70/00)
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@SubShelfCode, N'Cavities', 170, 0, N'1'),
			(@SubShelfCode, N'Colour', 20, 0, @MaterialTypeNorm),
			(@SubShelfCode, N'Colour Number', 10, 0, N'-'),
			(@SubShelfCode, N'Count Type', 50, 0, N'Weigh Count'),
			(@SubShelfCode, N'Drawing Issue', 40, 0, N'1'),
			(@SubShelfCode, N'Drawing Number', 30, 0, N'321554-01'),
			(@SubShelfCode, N'Impressions', 180, 0, N'1'),
			(@SubShelfCode, N'Label Type', 70, 0, N'Route Card'),
			(@SubShelfCode, N'Location', 150, 0, N'STORES'),
			(@SubShelfCode, N'Pack Type', 60, 0, N'Assembled'),
			(@SubShelfCode, N'Part Weight', 160, 0, N'175g'),
			(@SubShelfCode, N'Quantity/Box', 80, 0, N'100'),
			(@SubShelfCode, N'Tool Number', 190, 0, N'1437');

		-- Back divider (equivalent to M/101/70/00)
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@BackDividerCode, N'Cavities', 170, 0, N'2'),
			(@BackDividerCode, N'Colour', 20, 0, @MaterialTypeNorm),
			(@BackDividerCode, N'Colour Number', 10, 0, N'-'),
			(@BackDividerCode, N'Count Type', 50, 0, N'Weigh Count'),
			(@BackDividerCode, N'Drawing Issue', 40, 0, N'1'),
			(@BackDividerCode, N'Drawing Number', 30, 0, N'321554-02'),
			(@BackDividerCode, N'Impressions', 180, 0, N'2'),
			(@BackDividerCode, N'Label Type', 70, 0, N'Route Card'),
			(@BackDividerCode, N'Location', 150, 0, N'STORES'),
			(@BackDividerCode, N'Pack Type', 60, 0, N'Assembled'),
			(@BackDividerCode, N'Part Weight', 160, 0, N'61g'),
			(@BackDividerCode, N'Quantity/Box', 80, 0, N'100'),
			(@BackDividerCode, N'Tool Number', 190, 0, N'1439');

		-- Wide foot (equivalent to M/97/70/00)
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@WideFootCode, N'Cavities', 170, 0, N'4'),
			(@WideFootCode, N'Colour', 20, 0, @MaterialTypeNorm),
			(@WideFootCode, N'Colour Number', 10, 0, N'-'),
			(@WideFootCode, N'Count Type', 50, 0, N'Weigh Count'),
			(@WideFootCode, N'Drawing Issue', 40, 0, N'1'),
			(@WideFootCode, N'Drawing Number', 30, 0, N'321554A'),
			(@WideFootCode, N'Impressions', 180, 0, N'4'),
			(@WideFootCode, N'Label Type', 70, 0, N'Route Card'),
			(@WideFootCode, N'Location', 150, 0, N'STORES'),
			(@WideFootCode, N'Pack Type', 60, 0, N'Assembled'),
			(@WideFootCode, N'Part Weight', 160, 0, N'171g'),
			(@WideFootCode, N'Quantity/Box', 80, 0, N'100'),
			(@WideFootCode, N'Tool Number', 190, 0, N'1440');

		-- Narrow foot (equivalent to M/99/70/00)
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@NarrowFootCode, N'Cavities', 170, 0, N'1'),
			(@NarrowFootCode, N'Colour', 20, 0, @MaterialTypeNorm),
			(@NarrowFootCode, N'Colour Number', 10, 0, N'-'),
			(@NarrowFootCode, N'Count Type', 50, 0, N'Weigh Count'),
			(@NarrowFootCode, N'Drawing Issue', 40, 0, N'1'),
			(@NarrowFootCode, N'Drawing Number', 30, 0, N'321554A'),
			(@NarrowFootCode, N'Impressions', 180, 0, N'1'),
			(@NarrowFootCode, N'Label Type', 70, 0, N'Route Card'),
			(@NarrowFootCode, N'Location', 150, 0, N'STORES'),
			(@NarrowFootCode, N'Pack Type', 60, 0, N'Assembled'),
			(@NarrowFootCode, N'Part Weight', 160, 0, N'171g'),
			(@NarrowFootCode, N'Quantity/Box', 80, 0, N'100'),
			(@NarrowFootCode, N'Tool Number', 190, 0, N'1441');

		-- Plastic material (equivalent to PC/999)
		INSERT INTO @Attr (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES
			(@MaterialPcCode, N'Colour', 50, 0, @MaterialTypeNorm),
			(@MaterialPcCode, N'Grade', 20, 0, N'303EP'),
			(@MaterialPcCode, N'Location', 60, 0, N'R2123-9'),
			(@MaterialPcCode, N'Material Type', 10, 0, N'PC'),
			(@MaterialPcCode, N'Name', 30, 0, N'Calibre'),
			(@MaterialPcCode, N'SG', 40, 0, N'1.21');

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
		-- Ops (match proc_DemoBom timings/offsets/durations)
		---------------------------------------------------------------------
		IF NOT EXISTS (SELECT 1 FROM Object.tbOp WHERE ObjectCode = @FgCode)
		BEGIN
			INSERT INTO Object.tbOp (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
			VALUES (@FgCode, 10, 0, 'ASSEMBLE', 0.5, 3),
				   (@FgCode, 20, 0, 'QUALITY CHECK', 0, 0),
				   (@FgCode, 30, 0, 'PACK', 0, 1),
				   (@FgCode, 40, 2, 'DELIVER', 0, 1);
		END

		IF NOT EXISTS (SELECT 1 FROM Object.tbOp WHERE ObjectCode = @SubShelfCode)
		BEGIN
			INSERT INTO Object.tbOp (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
			VALUES (@SubShelfCode, 10, 0, 'MOULD', 10, 2),
				   (@SubShelfCode, 20, 1, 'INSERTS', 0, 0),
				   (@SubShelfCode, 30, 0, 'QUALITY CHECK', 0, 0);
		END

		IF NOT EXISTS (SELECT 1 FROM Object.tbOp WHERE ObjectCode = @BackDividerCode)
		BEGIN
			INSERT INTO Object.tbOp (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
			VALUES (@BackDividerCode, 10, 0, 'MOULD', 10, 0),
				   (@BackDividerCode, 20, 0, 'QUALITY CHECK', 0, 0);
		END

		IF NOT EXISTS (SELECT 1 FROM Object.tbOp WHERE ObjectCode = @WideFootCode)
		BEGIN
			INSERT INTO Object.tbOp (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
			VALUES (@WideFootCode, 10, 0, 'MOULD', 10, 2),
				   (@WideFootCode, 20, 0, 'QUALITY CHECK', 0, 0);
		END

		IF NOT EXISTS (SELECT 1 FROM Object.tbOp WHERE ObjectCode = @NarrowFootCode)
		BEGIN
			INSERT INTO Object.tbOp (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
			VALUES (@NarrowFootCode, 10, 0, 'MOULD', 0, 2),
				   (@NarrowFootCode, 20, 0, 'QUALITY CHECK', 0, 0);
		END

		---------------------------------------------------------------------
		-- BOM Flow (same structure/quantities/sync types/offsets as proc_DemoBom)
		---------------------------------------------------------------------
		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @FgCode AND StepNumber = 10)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@FgCode, 10, @SubShelfCode, 1, 0, 8);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @FgCode AND StepNumber = 20)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@FgCode, 20, @BackDividerCode, 1, 0, 4);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @FgCode AND StepNumber = 30)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@FgCode, 30, @WideFootCode, 1, 0, 3);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @FgCode AND StepNumber = 40)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@FgCode, 40, @NarrowFootCode, 0, 0, 2);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @FgCode AND StepNumber = 50)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@FgCode, 50, @Box41Code, 1, 0, 1);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @FgCode AND StepNumber = 60)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@FgCode, 60, @PalletCode, 1, 0, 0.01);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @FgCode AND StepNumber = 70)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@FgCode, 70, @DeliveryCode, 2, 1, 0);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @SubShelfCode AND StepNumber = 10)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@SubShelfCode, 10, @Box99Code, 1, 0, 0.01);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @SubShelfCode AND StepNumber = 20)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@SubShelfCode, 20, @MaterialPcCode, 1, 0, 0.175);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @SubShelfCode AND StepNumber = 30)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@SubShelfCode, 30, @Insert09Code, 1, 0, 2);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @BackDividerCode AND StepNumber = 10)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@BackDividerCode, 10, @Box99Code, 1, 0, 0.01);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @BackDividerCode AND StepNumber = 20)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@BackDividerCode, 20, @MaterialPcCode, 1, 0, 0.061);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @WideFootCode AND StepNumber = 10)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@WideFootCode, 10, @Box99Code, 1, 0, 0.01);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @WideFootCode AND StepNumber = 20)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@WideFootCode, 20, @MaterialPcCode, 1, 0, 0.172);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @NarrowFootCode AND StepNumber = 10)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@NarrowFootCode, 10, @Box99Code, 1, 0, 0.01);

		IF NOT EXISTS (SELECT 1 FROM Object.tbFlow WHERE ParentCode = @NarrowFootCode AND StepNumber = 20)
			INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
			VALUES (@NarrowFootCode, 20, @MaterialPcCode, 1, 0, 0.171);

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
GO
