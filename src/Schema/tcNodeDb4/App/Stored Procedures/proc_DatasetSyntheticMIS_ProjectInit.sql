CREATE PROCEDURE App.proc_DatasetSyntheticMIS_ProjectInit
(
	@IsCompany bit,
	@IsVatRegistered bit,
    @EnableOpeningBalance bit = 1
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51210, 'DatasetSyntheticMIS_ProjectInit: #DatasetCodes was not found. Run via App.proc_DatasetSyntheticMIS.', 1;

	DECLARE
		@Code nvarchar(50),
		@BusinessRootCode nvarchar(50) = (SELECT TOP (1) SubjectCode FROM App.tbOptions),
		@BuyerRootCode nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'BuyerRoot'),
		@SellerRootCode nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'SellerRoot');

	IF @BuyerRootCode IS NULL
		THROW 51212, 'DatasetSyntheticMIS_ProjectInit: missing SUBJECT/BuyerRoot in #DatasetCodes.', 1;

	IF @SellerRootCode IS NULL
		THROW 51213, 'DatasetSyntheticMIS_ProjectInit: missing SUBJECT/SellerRoot in #DatasetCodes.', 1;

	DECLARE @Company AS TABLE
	(
		CodeName nvarchar(100) NOT NULL,
		SubjectName nvarchar(100) NOT NULL,
		RootCodeName nvarchar(100) NOT NULL,
		SubjectTypeCode smallint NOT NULL,
		TaxCode nvarchar(10) NOT NULL,
		EUJurisdiction bit NOT NULL,
		PaymentTerms nvarchar(100) NOT NULL,
		ExpectedDays smallint NOT NULL,
		PaymentDays smallint NOT NULL,
		PayDaysFromMonthEnd bit NOT NULL,
		PayBalance bit NOT NULL,
		ContactName nvarchar(100) NOT NULL
	);

	INSERT INTO @Company
	(
		CodeName, SubjectName, RootCodeName, SubjectTypeCode, TaxCode, EUJurisdiction,
		PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance, ContactName
	)
	VALUES
		(N'MouldingCustomerUK', N'Dataset Moulding Customer UK', N'BuyerRoot', 1, N'T1', 0, N'30 days', 0, 30, 0, 1, N'Alice Turner'),
		(N'MouldingCustomerEU', N'Dataset Moulding Customer EU', N'BuyerRoot', 1, N'T0', 1, N'30 days', 0, 30, 0, 1, N'Lucas Meyer'),
		(N'PrintCustomerUK', N'Dataset Print Customer UK', N'BuyerRoot', 1, N'T1', 0, N'30 days', 0, 30, 0, 1, N'Emma Clarke'),
		(N'PrintCustomerEU', N'Dataset Print Customer EU', N'BuyerRoot', 1, N'T0', 1, N'30 days', 0, 30, 0, 1, N'Sofia Laurent'),
		(N'MiscCustomer1', N'Dataset Walk-in Customer', N'BuyerRoot', 1, N'T1', 0, N'Immediate', 0, 0, 0, 1, N'Mason Reed'),
		(N'MiscCustomer2', N'Dataset Online Customer', N'BuyerRoot', 1, N'T1', 0, N'14 days', 0, 14, 0, 1, N'Olivia Hart'),
		(N'PlasticSupplier', N'Dataset Plastic Supplier', N'SellerRoot', 0, N'T1', 0, N'30 days', 0, 30, 0, 1, N'Peter Walsh'),
		(N'InsertSupplier', N'Dataset Inserts Supplier', N'SellerRoot', 0, N'T1', 0, N'30 days', 0, 30, 0, 1, N'Nina Shah'),
		(N'BoxSupplier', N'Dataset Boxes & Pallets Supplier', N'SellerRoot', 0, N'T1', 0, N'30 days', 0, 30, 0, 1, N'Daniel Brooks'),
		(N'MouldingHaulier', N'Dataset Haulier (Moulding)', N'SellerRoot', 0, N'T1', 0, N'30 days end of month', 0, 30, 1, 1, N'Gareth Miles'),
		(N'Printer', N'Dataset Printer', N'SellerRoot', 0, N'T1', 0, N'30 days', 0, 30, 0, 1, N'Harriet Cole'),
		(N'PrintHaulier', N'Dataset Haulier (Print)', N'SellerRoot', 0, N'T1', 0, N'30 days end of month', 0, 30, 1, 1, N'Connor Webb'),
		(N'ProvisionsSupplier', N'Dataset Provisions Supplier', N'SellerRoot', 0, N'T1', 0, N'30 days', 0, 30, 0, 1, N'Rachel Green'),
		(N'EntertainmentSupplier', N'Dataset Entertainment Supplier', N'SellerRoot', 0, N'T1', 0, N'14 days', 0, 14, 0, 1, N'Lewis Grant'),
		(N'VehicleMaintenanceSupplier', N'Dataset Vehicle Maintenance Supplier', N'SellerRoot', 0, N'T1', 0, N'30 days', 0, 30, 0, 1, N'Martin Cross');

	DECLARE
		@CompanyCodeName nvarchar(100),
		@CompanyName nvarchar(100),
		@CompanyRootCodeName nvarchar(100),
		@CompanyRootCode nvarchar(50),
		@CompanySubjectTypeCode smallint,
		@CompanyTaxCode nvarchar(10),
		@CompanyEU bit,
		@CompanyTerms nvarchar(100),
		@CompanyExpectedDays smallint,
		@CompanyPaymentDays smallint,
		@CompanyMonthEnd bit,
		@CompanyPayBalance bit,
		@CompanyContactName nvarchar(100);

	DECLARE curCompanies CURSOR LOCAL FAST_FORWARD FOR
		SELECT
			CodeName,
			SubjectName,
			RootCodeName,
			SubjectTypeCode,
			TaxCode,
			EUJurisdiction,
			PaymentTerms,
			ExpectedDays,
			PaymentDays,
			PayDaysFromMonthEnd,
			PayBalance,
			ContactName
		FROM @Company;

	OPEN curCompanies;
	FETCH NEXT FROM curCompanies INTO
		@CompanyCodeName,
		@CompanyName,
		@CompanyRootCodeName,
		@CompanySubjectTypeCode,
		@CompanyTaxCode,
		@CompanyEU,
		@CompanyTerms,
		@CompanyExpectedDays,
		@CompanyPaymentDays,
		@CompanyMonthEnd,
		@CompanyPayBalance,
		@CompanyContactName;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @Code = NULL;

		SET @CompanyRootCode =
			CASE @CompanyRootCodeName
				WHEN N'BuyerRoot' THEN @BuyerRootCode
				WHEN N'SellerRoot' THEN @SellerRootCode
			END;

		IF @CompanyRootCode IS NULL
			THROW 51214, 'DatasetSyntheticMIS_ProjectInit: company root was not resolved.', 1;

		EXEC Subject.proc_AddNamespace
			@RootSubjectCode = @CompanyRootCode,
			@SubjectName = @CompanyName,
			@SubjectTypeCode = @CompanySubjectTypeCode,
			@SubjectCode = @Code OUTPUT;

		UPDATE Subject.tbSubject
		SET
			SubjectStatusCode = 1,
			TaxCode = @CompanyTaxCode,
			PaymentTerms = @CompanyTerms,
			ExpectedDays = @CompanyExpectedDays,
			PaymentDays = @CompanyPaymentDays,
			PayDaysFromMonthEnd = @CompanyMonthEnd,
			PayBalance = @CompanyPayBalance
		WHERE SubjectCode = @Code;

		UPDATE Subject.tbVirtual
		SET EUJurisdiction = @CompanyEU
		WHERE SubjectCode = @Code;

		EXEC Subject.proc_AddContact
			@SubjectCode = @Code,
			@ContactName = @CompanyContactName;

		MERGE #DatasetCodes AS t
		USING (SELECT N'SUBJECT', @CompanyCodeName, @Code, @CompanyRootCodeName, @CompanyName) AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
			ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
		WHEN NOT MATCHED THEN
			INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes)
			VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
		WHEN MATCHED THEN
			UPDATE SET
				CodeValue = s.CodeValue,
				RelatedName = s.RelatedName,
				Notes = s.Notes;

		FETCH NEXT FROM curCompanies INTO
			@CompanyCodeName,
			@CompanyName,
			@CompanyRootCodeName,
			@CompanySubjectTypeCode,
			@CompanyTaxCode,
			@CompanyEU,
			@CompanyTerms,
			@CompanyExpectedDays,
			@CompanyPaymentDays,
			@CompanyMonthEnd,
			@CompanyPayBalance,
			@CompanyContactName;
	END

	CLOSE curCompanies;
	DEALLOCATE curCompanies;

	SET @Code = NULL;

	EXEC Subject.proc_AddNamespace
		@RootSubjectCode = @BusinessRootCode,
		@SubjectName = N'John Smith',
		@SubjectTypeCode = 9,
		@SubjectCode = @Code OUTPUT;

	UPDATE Subject.tbSubject
	SET
		SubjectStatusCode = 1,
		TaxCode = N'N/A',
		PaymentTerms = N'Immediate',
		ExpectedDays = 0,
		PaymentDays = 0,
		PayDaysFromMonthEnd = 0,
		PayBalance = 1
	WHERE SubjectCode = @Code;

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'Employee', @Code, NULL, N'John Smith') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	DECLARE
		@AddrSubjectCode nvarchar(50),
		@AddrSubjectName nvarchar(100);

	DECLARE curAddresses CURSOR LOCAL FAST_FORWARD FOR
		SELECT
			CAST(dc.CodeValue AS nvarchar(50)) AS SubjectCode,
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

			SET @Address =
				CASE WHEN @AddrSubjectCode = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'Employee')
					THEN N'Residence of ' + @AddrSubjectName
					ELSE N'Address of ' + @AddrSubjectName
				END;

			EXEC Subject.proc_AddAddress
				@SubjectCode = @AddrSubjectCode,
				@Address = @Address;
		END

		FETCH NEXT FROM curAddresses INTO @AddrSubjectCode, @AddrSubjectName;
	END

	CLOSE curAddresses;
	DEALLOCATE curAddresses;

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

	DECLARE
		@OpeningAR decimal(18,5) = (CASE @EnableOpeningBalance WHEN 1 THEN 1200.00000 ELSE 0 END),
		@OpeningAP decimal(18,5) = (CASE @EnableOpeningBalance WHEN 1 THEN -800.00000 ELSE 0 END);

	DECLARE
		@OpeningCustomerCode nvarchar(50) =
			(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerUK'),
		@OpeningSupplierCode nvarchar(50) =
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
GO
