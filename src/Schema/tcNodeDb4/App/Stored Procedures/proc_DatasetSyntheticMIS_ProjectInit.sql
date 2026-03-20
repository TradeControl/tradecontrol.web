CREATE PROCEDURE App.proc_DatasetSyntheticMIS_ProjectInit
(
	@IsCompany bit,
	@IsVatRegistered bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51210, 'DatasetSyntheticMIS_ProjectInit: #DatasetCodes was not found. Run via App.proc_DatasetSyntheticMIS.', 1;

	---------------------------------------------------------------------
	-- Helper vars
	---------------------------------------------------------------------
	DECLARE @Code nvarchar(10);

	---------------------------------------------------------------------
	-- Customers (UK + EU + misc)
	---------------------------------------------------------------------
	-- Moulding customer UK (T1, not EU)
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Moulding Customer UK', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Moulding Customer UK', 1, 1,
			N'T1', 0,
			N'30 days', 0, 30, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT' AS CodeType, N'MouldingCustomerUK' AS CodeName, @Code AS CodeValue, NULL AS RelatedName, N'Dataset Moulding Customer UK' AS Notes) AS s
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Moulding customer EU (T0, EUJurisdiction=1)
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Moulding Customer EU', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Moulding Customer EU', 1, 1,
			N'T0', 1,
			N'30 days', 0, 30, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'MouldingCustomerEU', @Code, NULL, N'Dataset Moulding Customer EU') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Print customer UK (T1)
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Print Customer UK', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Print Customer UK', 1, 1,
			N'T1', 0,
			N'30 days', 0, 30, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'PrintCustomerUK', @Code, NULL, N'Dataset Print Customer UK') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Print customer EU (T0)
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Print Customer EU', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Print Customer EU', 1, 1,
			N'T0', 1,
			N'30 days', 0, 30, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'PrintCustomerEU', @Code, NULL, N'Dataset Print Customer EU') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Misc customers
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Walk-in Customer', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Walk-in Customer', 1, 1,
			N'T1', 0,
			N'Immediate', 0, 0, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'MiscCustomer1', @Code, NULL, N'Dataset Walk-in Customer') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Online Customer', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Online Customer', 1, 1,
			N'T1', 0,
			N'14 days', 0, 14, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'MiscCustomer2', @Code, NULL, N'Dataset Online Customer') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	---------------------------------------------------------------------
	-- Ensure mandatory addresses exist
	---------------------------------------------------------------------
	DECLARE
		@AddrSubjectCode nvarchar(10),
		@AddrSubjectName nvarchar(100);

	DECLARE curAddresses CURSOR LOCAL FAST_FORWARD FOR
		SELECT
			CAST(dc.CodeValue AS nvarchar(10)) AS SubjectCode,
			CAST(dc.Notes AS nvarchar(100)) AS SubjectName
		FROM #DatasetCodes dc
		WHERE dc.CodeType = N'SUBJECT'
			AND dc.CodeName IN
			(
			N'MouldingCustomerUK',
			N'MouldingCustomerEU',
			N'PrintCustomerUK',
			N'PrintCustomerEU',
			N'MiscCustomer1',
			N'MiscCustomer2',
			N'PlasticSupplier',
			N'InsertSupplier',
			N'BoxSupplier',
			N'MouldingHaulier',
			N'Printer',
			N'PrintHaulier',
			N'ProvisionsSupplier',
			N'EntertainmentSupplier',
			N'VehicleMaintenanceSupplier',
			N'Employee'
			);

	OPEN curAddresses;
	FETCH NEXT FROM curAddresses INTO @AddrSubjectCode, @AddrSubjectName;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS
		(
			SELECT 1
			FROM Subject.tbSubject s
			JOIN Subject.tbAddress a ON a.AddressCode = s.AddressCode
			WHERE s.SubjectCode = @AddrSubjectCode
		)
		BEGIN
			DECLARE @Address nvarchar(max);
			SET @Address = N'Residence of ' + @AddrSubjectName;

			EXEC Subject.proc_AddAddress
				@SubjectCode = @AddrSubjectCode,
				@Address = @Address;
		END

		FETCH NEXT FROM curAddresses INTO @AddrSubjectCode, @AddrSubjectName;
	END

	CLOSE curAddresses;
	DEALLOCATE curAddresses;

	---------------------------------------------------------------------
	-- Suppliers
	---------------------------------------------------------------------
	DECLARE @Supplier AS TABLE
	(
		CodeName nvarchar(100) NOT NULL,
		SubjectName nvarchar(100) NOT NULL,
		PaymentTerms nvarchar(100) NOT NULL,
		PayDaysFromMonthEnd bit NOT NULL
	);

	INSERT INTO @Supplier (CodeName, SubjectName, PaymentTerms, PayDaysFromMonthEnd)
	VALUES
		(N'PlasticSupplier', N'Dataset Plastic Supplier', N'30 days', 0),
		(N'InsertSupplier', N'Dataset Inserts Supplier', N'30 days', 0),
		(N'BoxSupplier', N'Dataset Boxes & Pallets Supplier', N'30 days', 0),
		(N'MouldingHaulier', N'Dataset Haulier (Moulding)', N'30 days end of month', 1),
		(N'Printer', N'Dataset Printer', N'30 days', 0),
		(N'PrintHaulier', N'Dataset Haulier (Print)', N'30 days end of month', 1),
		(N'ProvisionsSupplier', N'Dataset Provisions Supplier', N'30 days', 0),
		(N'EntertainmentSupplier', N'Dataset Entertainment Supplier', N'14 days', 0),
		(N'VehicleMaintenanceSupplier', N'Dataset Vehicle Maintenance Supplier', N'30 days', 0);

	DECLARE
		@SuppCodeName nvarchar(100),
		@SuppName nvarchar(100),
		@SuppTerms nvarchar(100),
		@SuppME bit;

	DECLARE c CURSOR LOCAL FAST_FORWARD FOR
		SELECT CodeName, SubjectName, PaymentTerms, PayDaysFromMonthEnd
		FROM @Supplier;

	OPEN c;
	FETCH NEXT FROM c INTO @SuppCodeName, @SuppName, @SuppTerms, @SuppME;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @Code = NULL;
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @SuppName, @SubjectCode = @Code OUTPUT;

		IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
		BEGIN
			INSERT INTO Subject.tbSubject
			(
				SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
				TaxCode, EUJurisdiction,
				PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
			)
			VALUES
			(
				@Code, @SuppName, 0, 1,
				N'T1', 0,
				@SuppTerms, 0, CASE WHEN @SuppTerms LIKE N'%14%' THEN 14 ELSE 30 END, @SuppME, 1
			);
		END

		MERGE #DatasetCodes AS t
		USING (SELECT N'SUBJECT', @SuppCodeName, @Code, NULL, @SuppName) AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
			ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
		WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
		WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

		FETCH NEXT FROM c INTO @SuppCodeName, @SuppName, @SuppTerms, @SuppME;
	END

	CLOSE c;
	DEALLOCATE c;

	---------------------------------------------------------------------
	-- Employee (SubjectTypeCode=9)
	---------------------------------------------------------------------
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'John Smith', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'John Smith', 9, 1,
			N'N/A', 0,
			N'Immediate', 0, 0, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'Employee', @Code, NULL, N'John Smith') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	---------------------------------------------------------------------
	-- Objects (3 colors + 2 services)
	---------------------------------------------------------------------
	DECLARE
		@WidgetClear nvarchar(50),
		@WidgetRed nvarchar(50),
		@WidgetBlue nvarchar(50),
		@ServiceFlyer nvarchar(50),
		@ServiceBrochure nvarchar(50);

	EXEC App.proc_DatasetCreateProduct @MaterialType = N'CLEAR', @ObjectCode = @WidgetClear OUTPUT;
	EXEC App.proc_DatasetCreateProduct @MaterialType = N'RED', @ObjectCode = @WidgetRed OUTPUT;
	EXEC App.proc_DatasetCreateProduct @MaterialType = N'BLUE', @ObjectCode = @WidgetBlue OUTPUT;

	EXEC App.proc_DatasetCreateService @ServiceName = N'Flyer', @UnitCharge = 0.5, @ObjectCode = @ServiceFlyer OUTPUT;
	EXEC App.proc_DatasetCreateService @ServiceName = N'Brochure', @UnitCharge = 0.10, @ObjectCode = @ServiceBrochure OUTPUT;

	MERGE #DatasetCodes AS t
	USING
	(
		SELECT N'OBJECT' AS CodeType, N'Widget_CLEAR' AS CodeName, @WidgetClear AS CodeValue, NULL AS RelatedName, N'' AS Notes
		UNION ALL SELECT N'OBJECT', N'Widget_RED', @WidgetRed, NULL, N''
		UNION ALL SELECT N'OBJECT', N'Widget_BLUE', @WidgetBlue, NULL, N''
		UNION ALL SELECT N'OBJECT', N'Service_Flyer', @ServiceFlyer, NULL, N''
		UNION ALL SELECT N'OBJECT', N'Service_Brochure', @ServiceBrochure, NULL, N''
		UNION ALL SELECT N'LINK', N'Service_Brochure_Printer', (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'Printer'), N'Service_Brochure', N'Primary printer supplier'
		UNION ALL SELECT N'LINK', N'PO_Transport_Haulier', (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintHaulier'), N'PO Transport', N'Primary haulier for PO Transport'
	) AS s
		ON t.CodeType = s.CodeType
		AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN
		INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes)
		VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN
		UPDATE SET
			CodeValue = s.CodeValue,
			RelatedName = s.RelatedName,
			Notes = s.Notes;

	---------------------------------------------------------------------
	-- Seed opening AR/AP
	---------------------------------------------------------------------
	DECLARE
		@OpeningAR decimal(18,5) = 1200.00000,
		@OpeningAP decimal(18,5) = -800.00000;

	DECLARE
		@OpeningCustomerCode nvarchar(10) =
			(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerUK'),
		@OpeningSupplierCode nvarchar(10) =
			(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PlasticSupplier');

	IF @OpeningCustomerCode IS NULL
		THROW 51020, 'SyntheticDataset: missing #DatasetCodes entry for SUBJECT/MouldingCustomerUK.', 1;

	IF @OpeningSupplierCode IS NULL
		THROW 51021, 'SyntheticDataset: missing #DatasetCodes entry for SUBJECT/PlasticSupplier.', 1;

	UPDATE Subject.tbSubject
	SET OpeningBalance = @OpeningAR
	WHERE SubjectCode = @OpeningCustomerCode;

	UPDATE Subject.tbSubject
	SET OpeningBalance = @OpeningAP
	WHERE SubjectCode = @OpeningSupplierCode;


