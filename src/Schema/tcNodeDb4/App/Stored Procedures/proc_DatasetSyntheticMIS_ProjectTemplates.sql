CREATE PROCEDURE App.proc_DatasetSyntheticMIS_ProjectTemplates
(
	@IsCompany bit,
	@IsVatRegistered bit,
    @PriceRatio decimal(18,7) = 1.0000000,          -- adjust selling price (profit/loss lever)
    @QuantityRatio decimal(18,7) = 1.0000000        --adjust demand
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51211, 'DatasetSyntheticMIS_ProjectTemplates: #DatasetCodes was not found. Run via App.proc_DatasetSyntheticMIS.', 1;

	DECLARE @DeadYearStartOn date =
	(
		SELECT MIN(yp.StartOn)
		FROM App.tbYear y
		JOIN App.tbYearPeriod yp ON yp.YearNumber = y.YearNumber
		WHERE y.YearNumber = (SELECT MIN(YearNumber) FROM App.tbYear)
	);

	DECLARE
        @Quantity decimal(18,7) = 100.0,
		@MouldingCustomerUK nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerUK'),
		@MouldingCustomerEU nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerEU'),
		@PrintCustomerUK nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintCustomerUK'),
		@PrintCustomerEU nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintCustomerEU'),

		@PlasticSupplier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PlasticSupplier'),
		@InsertSupplier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'InsertSupplier'),
		@BoxSupplier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'BoxSupplier'),
		@MouldingHaulier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingHaulier'),
		@Printer nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'Printer'),
		@PrintHaulier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintHaulier'),

		@WidgetClearObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Widget_CLEAR'),
		@WidgetRedObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Widget_RED'),
		@WidgetBlueObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Widget_BLUE'),
		@ServiceFlyerObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Service_Flyer'),
		@ServiceBrochureObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Service_Brochure');

	DECLARE
		@ContainerProjectCode nvarchar(20),
		@TemplateProjectCode nvarchar(20);

    SET @Quantity = CAST(ROUND(@Quantity * @QuantityRatio, 0) AS decimal(18,7));

	-- Moulding UK container + 3 templates
	SET @ContainerProjectCode = NULL;

	INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
	VALUES (N'PROJECT', 0, NULL, N'each', NULL, 0, 0, N'Works Order');

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Moulding UK',
		@CustomerSubjectCode = @MouldingCustomerUK,
		@ObjectCode = @WidgetClearObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = @Quantity,
        @PriceRatio = @PriceRatio,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT' AS CodeType, N'TPL_MouldingUK_CLEAR' AS CodeName, @TemplateProjectCode AS CodeValue, N'MouldingCustomerUK' AS RelatedName, N'' AS Notes) s
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Moulding UK',
		@CustomerSubjectCode = @MouldingCustomerUK,
		@ObjectCode = @WidgetRedObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = @Quantity,
        @PriceRatio = @PriceRatio,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT', N'TPL_MouldingUK_RED', @TemplateProjectCode, N'MouldingCustomerUK', N'') s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Moulding UK',
		@CustomerSubjectCode = @MouldingCustomerUK,
		@ObjectCode = @WidgetBlueObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = @Quantity,
        @PriceRatio = @PriceRatio,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT', N'TPL_MouldingUK_BLUE', @TemplateProjectCode, N'MouldingCustomerUK', N'') s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Print UK container + 2 templates
	SET @ContainerProjectCode = NULL;

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Print UK',
		@CustomerSubjectCode = @PrintCustomerUK,
		@ObjectCode = @ServiceFlyerObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = @Quantity,
        @PriceRatio = @PriceRatio,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT', N'TPL_PrintUK_Flyer', @TemplateProjectCode, N'PrintCustomerUK', N'') s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

    SET @Quantity *= 100;

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Print UK',
		@CustomerSubjectCode = @PrintCustomerUK,
		@ObjectCode = @ServiceBrochureObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = @Quantity,
        @PriceRatio = @PriceRatio,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT', N'TPL_PrintUK_Brochure', @TemplateProjectCode, N'PrintCustomerUK', N'') s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	---------------------------------------------------------------------
	-- Initialise delivery/carriage legs (manual quantity because UsedOnQuantity = 0)
	---------------------------------------------------------------------
	UPDATE p
	SET
		Quantity = 1,
		UnitCharge = o.UnitCharge,
		TotalCharge = o.UnitCharge * 1
	FROM Project.tbFlow f
		JOIN Project.tbProject p
			ON p.ProjectCode = f.ChildProjectCode
			AND p.CashCode = N'TC200'
		JOIN Object.tbObject o
			ON p.ObjectCode = o.ObjectCode
	WHERE f.UsedOnQuantity = 0;

