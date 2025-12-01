CREATE PROCEDURE App.proc_DemoBom
(
	@CreateOrders BIT = 0,
	@InvoiceOrders BIT = 0,
	@PayInvoices BIT = 0
)
AS
	 SET NOCOUNT, XACT_ABORT ON;
	 
	 BEGIN TRY
	
		IF NOT EXISTS (SELECT * FROM Usr.vwCredentials WHERE IsAdministrator <> 0)
		BEGIN
			DECLARE @Msg NVARCHAR(100) = CONCAT('Access Denied: User ', SUSER_SNAME(), ' is not an administrsator');
			RAISERROR ('%s', 13, 1, @Msg);
		END
	
		DECLARE @ExchangeRate float = CASE (SELECT UnitOfCharge FROM App.tbOptions) 
										WHEN 'BTC' THEN 0.135 
										ELSE 1 
										END				

		BEGIN TRAN

		-->>>>>>>>>>>>> RESET >>>>>>>>>>>>>>>>>>>>>>>>>>>
		DELETE FROM Cash.tbPayment;
		DELETE FROM Invoice.tbInvoice;
		DELETE FROM Project.tbFlow;
		DELETE FROM Project.tbProject;
		DELETE FROM Object.tbFlow;
		DELETE FROM Object.tbObject;

		--WITH sys_accounts AS
		--(
		--	SELECT SubjectCode FROM App.tbOptions
		--	UNION
		--	SELECT DISTINCT SubjectCode FROM Subject.tbAccount
		--	UNION
		--	SELECT DISTINCT SubjectCode FROM Cash.tbTaxType
		--), candidates AS
		--(
		--	SELECT SubjectCode
		--	FROM Subject.tbSubject
		--	EXCEPT
		--	SELECT SubjectCode 
		--	FROM sys_accounts
		--)
		--DELETE Subject.tbSubject 
		--FROM Subject.tbSubject JOIN candidates ON Subject.tbSubject.SubjectCode = candidates.SubjectCode;
		
		UPDATE App.tbOptions
		SET IsAutoOffsetDays = 0;

		EXEC App.proc_SystemRebuild;
		--<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

		IF NOT EXISTS( SELECT * FROM App.tbRegister WHERE RegisterName = 'Works Order')
			INSERT INTO App.tbRegister (RegisterName, NextNumber)
			SELECT 'Works Order', (SELECT MAX(NextNumber) + 10000 FROM App.tbRegister) AS NextNumber;

		INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
		VALUES ('M/00/70/00', 1, 'PIGEON HOLE SHELF ASSEMBLY CLEAR', 'each', '103', 1.67 * @ExchangeRate, 1, 'Sales Order')
		, ('M/100/70/00', 1, 'PIGEON HOLE SUB SHELF CLEAR', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('M/101/70/00', 1, 'PIGEON HOLE BACK DIVIDER', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('M/97/70/00', 1, 'SHELF DIVIDER (WIDE FOOT)', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('M/99/70/00', 1, 'SHELF DIVIDER (NARROW FOOT)', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('PALLET/01', 1, 'EURO 3 1200 x 800 4 WAY', 'each', '200', 2.4 * @ExchangeRate, 1, 'Purchase Order')
		, ('BOX/41', 1, 'PIGEON ASSY 125KTB S WALL 404x220x90', 'each', '200', 0.05 * @ExchangeRate, 1, 'Purchase Order')
		, ('BOX/99', 1, 'INTERNAL USE ANY BLACK,BLUE,RED ANY', 'each', NULL, 0.0000, 0, 'Works Order')
		, ('PC/999', 1, 'CALIBRE 303EP CLEAR UL94-V2', 'kilo', '200', 0.22 * @ExchangeRate, 1, 'Purchase Order')
		, ('INSERT/09', 1, 'HEAT-LOK SHK B M3.5 HEADED BRASS 8620035-80', 'each', '200', 0.005 * @ExchangeRate, 1, 'Purchase Order')
		, ('PROJECT', 0, NULL, 'each', NULL, 0, 0, 'Works Order')
		, ('DELIVERY', 1, NULL, 'each', '200', 0, 1, 'Purchase Order')
		;
		INSERT INTO Object.tbAttribute (ObjectCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
		VALUES ('M/00/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/00/70/00', 'Colour Number', 10, 0, '-')
		, ('M/00/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/00/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/00/70/00', 'Drawing Number', 30, 0, '321554')
		, ('M/00/70/00', 'Label Type', 70, 0, 'Assembly Card')
		, ('M/00/70/00', 'Mould Tool Specification', 110, 1, NULL)
		, ('M/00/70/00', 'Pack Type', 60, 0, 'Despatched')
		, ('M/00/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/100/70/00', 'Cavities', 170, 0, '1')
		, ('M/100/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/100/70/00', 'Colour Number', 10, 0, '-')
		, ('M/100/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/100/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/100/70/00', 'Drawing Number', 30, 0, '321554-01')
		, ('M/100/70/00', 'Impressions', 180, 0, '1')
		, ('M/100/70/00', 'Label Type', 70, 0, 'Route Card')
		, ('M/100/70/00', 'Location', 150, 0, 'STORES')
		, ('M/100/70/00', 'Pack Type', 60, 0, 'Assembled')
		, ('M/100/70/00', 'Part Weight', 160, 0, '175g')
		, ('M/100/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/100/70/00', 'Tool Number', 190, 0, '1437')
		, ('M/101/70/00', 'Cavities', 170, 0, '2')
		, ('M/101/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/101/70/00', 'Colour Number', 10, 0, '-')
		, ('M/101/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/101/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/101/70/00', 'Drawing Number', 30, 0, '321554-02')
		, ('M/101/70/00', 'Impressions', 180, 0, '2')
		, ('M/101/70/00', 'Label Type', 70, 0, 'Route Card')
		, ('M/101/70/00', 'Location', 150, 0, 'STORES')
		, ('M/101/70/00', 'Pack Type', 60, 0, 'Assembled')
		, ('M/101/70/00', 'Part Weight', 160, 0, '61g')
		, ('M/101/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/101/70/00', 'Tool Number', 190, 0, '1439')
		, ('M/97/70/00', 'Cavities', 170, 0, '4')
		, ('M/97/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/97/70/00', 'Colour Number', 10, 0, '-')
		, ('M/97/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/97/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/97/70/00', 'Drawing Number', 30, 0, '321554A')
		, ('M/97/70/00', 'Impressions', 180, 0, '4')
		, ('M/97/70/00', 'Label Type', 70, 0, 'Route Card')
		, ('M/97/70/00', 'Location', 150, 0, 'STORES')
		, ('M/97/70/00', 'Pack Type', 60, 0, 'Assembled')
		, ('M/97/70/00', 'Part Weight', 160, 0, '171g')
		, ('M/97/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/97/70/00', 'Tool Number', 190, 0, '1440')
		, ('M/99/70/00', 'Cavities', 170, 0, '1')
		, ('M/99/70/00', 'Colour', 20, 0, 'CLEAR')
		, ('M/99/70/00', 'Colour Number', 10, 0, '-')
		, ('M/99/70/00', 'Count Type', 50, 0, 'Weigh Count')
		, ('M/99/70/00', 'Drawing Issue', 40, 0, '1')
		, ('M/99/70/00', 'Drawing Number', 30, 0, '321554A')
		, ('M/99/70/00', 'Impressions', 180, 0, '1')
		, ('M/99/70/00', 'Label Type', 70, 0, 'Route Card')
		, ('M/99/70/00', 'Location', 150, 0, 'STORES')
		, ('M/99/70/00', 'Pack Type', 60, 0, 'Assembled')
		, ('M/99/70/00', 'Part Weight', 160, 0, '171g')
		, ('M/99/70/00', 'Quantity/Box', 80, 0, '100')
		, ('M/99/70/00', 'Tool Number', 190, 0, '1441')
		, ('PC/999', 'Colour', 50, 0, 'CLEAR')
		, ('PC/999', 'Grade', 20, 0, '303EP')
		, ('PC/999', 'Location', 60, 0, 'R2123-9')
		, ('PC/999', 'Material Type', 10, 0, 'PC')
		, ('PC/999', 'Name', 30, 0, 'Calibre')
		, ('PC/999', 'SG', 40, 0, '1.21')
		;
		INSERT INTO Object.tbOp (ObjectCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
		VALUES ('M/00/70/00', 10, 0, 'ASSEMBLE', 0.5, 3)
		, ('M/00/70/00', 20, 0, 'QUALITY CHECK', 0, 0)
		, ('M/00/70/00', 30, 0, 'PACK', 0, 1)
		, ('M/00/70/00', 40, 2, 'DELIVER', 0, 1)
		, ('M/100/70/00', 10, 0, 'MOULD', 10, 2)
		, ('M/100/70/00', 20, 1, 'INSERTS', 0, 0)
		, ('M/100/70/00', 30, 0, 'QUALITY CHECK', 0, 0)
		, ('M/101/70/00', 10, 0, 'MOULD', 10, 0)
		, ('M/101/70/00', 20, 0, 'QUALITY CHECK', 0, 0)
		, ('M/97/70/00', 10, 0, 'MOULD', 10, 2)
		, ('M/97/70/00', 20, 0, 'QUALITY CHECK', 0, 0)
		, ('M/99/70/00', 10, 0, 'MOULD', 0, 2)
		, ('M/99/70/00', 20, 0, 'QUALITY CHECK', 0, 0)
		;
		INSERT INTO Object.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
		VALUES ('M/00/70/00', 10, 'M/100/70/00', 1, 0, 8)
		, ('M/00/70/00', 20, 'M/101/70/00', 1, 0, 4)
		, ('M/00/70/00', 30, 'M/97/70/00', 1, 0, 3)
		, ('M/00/70/00', 40, 'M/99/70/00', 0, 0, 2)
		, ('M/00/70/00', 50, 'BOX/41', 1, 0, 1)
		, ('M/00/70/00', 60, 'PALLET/01', 1, 0, 0.01)
		, ('M/00/70/00', 70, 'DELIVERY', 2, 1, 0)
		, ('M/100/70/00', 10, 'BOX/99', 1, 0, 0.01)
		, ('M/100/70/00', 20, 'PC/999', 1, 0, 0.175)
		, ('M/101/70/00', 10, 'BOX/99', 1, 0, 0.01)
		, ('M/101/70/00', 20, 'PC/999', 1, 0, 0.061)
		, ('M/97/70/00', 10, 'BOX/99', 1, 0, 0.01)
		, ('M/97/70/00', 20, 'PC/999', 1, 0, 0.172)
		, ('M/99/70/00', 10, 'BOX/99', 1, 0, 0.01)
		, ('M/99/70/00', 20, 'PC/999', 1, 0, 0.171)
		, ('M/100/70/00', 30, 'INSERT/09', 1, 0, 2)
		;

		IF (NOT EXISTS(SELECT * FROM Subject.tbSubject WHERE SubjectCode = 'TFCSPE'))
		BEGIN
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode, TaxCode, AddressCode, PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance, NumberOfEmployees, CompanyNumber, VatNumber, Turnover, OpeningBalance, EUJurisdiction)
			VALUES 
			  ('PACSER', 'PACKING SERVICES', 8, 1, 'T1', 'PACSER_001', 'EOM', 10, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('PALSUP', 'PALLET SUPPLIER', 8, 1, 'T1', 'PALSUP_001', 'COD', 0, -10, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('PLAPRO', 'PLASTICS PROVIDER', 8, 1, 'T1', 'PLAPRO_001', '30 days from invoice', 15, 30, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('TFCSPE', 'FASTENER SPECIALIST', 8, 1, 'T1', 'TFCSPE_001', 'EOM', 0, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('STOBOX', 'STORAGE BOXES', 1, 1, 'T1', 'STOBOX_001', '60 days from invoice', 5, 60, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('HAULOG', 'HAULIER LOGISTICS', 8, 1, 'T1', 'HAULOG_001', 'EOM', 0, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			;
			INSERT INTO Subject.tbAddress (AddressCode, SubjectCode, Address)
			VALUES ('STOBOX_001', 'STOBOX', 'SURREY GU24 9BJ')
			, ('PACSER_001', 'PACSER', 'FAREHAM, HAMPSHIRE	PO15 5RZ')
			, ('PLAPRO_001', 'PLAPRO', 'WARRINGTON, CHESHIRE WA1 4RA')
			, ('PALSUP_001', 'PALSUP', 'HAMPSHIRE PO13 9NY')
			, ('TFCSPE_001', 'TFCSPE', 'ESSEX CO4 9TZ')
			, ('HAULOG_001', 'HAULOG', 'BERKSHIRE SL3 0BH')
			;
		END

		-- ***************************************************************************
		IF @CreateOrders = 0
			GOTO CommitTran;
		-- ***************************************************************************

		DECLARE @UserId NVARCHAR(10) = (SELECT UserId FROM Usr.vwCredentials),
			@ProjectCode NVARCHAR(20),
			@ParentProjectCode NVARCHAR(20), 
			@ToProjectCode NVARCHAR(20),
			@Quantity DECIMAL(18, 4) = 100;

		EXEC Project.proc_NextCode 'PROJECT', @ParentProjectCode OUTPUT
		INSERT INTO Project.tbProject
								 (ProjectCode, UserId, SubjectCode, ProjectTitle, ObjectCode, ProjectStatusCode, ActionById)
		VALUES        (@ParentProjectCode,@UserId, 'STOBOX', N'PIGEON HOLE SHELF ASSEMBLY', N'PROJECT', 0,@UserId)
	
		EXEC Project.proc_NextCode 'M/00/70/00', @ProjectCode OUTPUT
		
		INSERT INTO Project.tbProject
				(ProjectCode, UserId, SubjectCode, ProjectTitle, ContactName, ObjectCode, ProjectStatusCode, ActionById, ProjectNotes, Quantity, CashCode, TaxCode, UnitCharge, AddressCodeFrom, AddressCodeTo, SecondReference, Printed)
		SELECT @ProjectCode,@UserId, 'STOBOX', ObjectDescription, 'Francis Brown', ObjectCode, 1,@UserId, ObjectDescription, @Quantity, '103', 'T1', UnitCharge, 'STOBOX_001', 'STOBOX_001', N'12354/2', 0		
		FROM Object.tbObject
		WHERE ObjectCode = 'M/00/70/00';

		EXEC Project.proc_Configure @ProjectCode;
		EXEC Project.proc_AssignToParent @ProjectCode, @ParentProjectCode;

	
		UPDATE Project.tbProject
		SET SubjectCode = 'PACSER', ContactName = 'John OGroats', AddressCodeFrom = 'PACSER_001', AddressCodeTo = 'PACSER_001'
		WHERE ObjectCode = 'BOX/41';

		UPDATE Project.tbProject
		SET SubjectCode = 'TFCSPE', ContactName = 'Gary Granger', AddressCodeFrom = 'TFCSPE_001', AddressCodeTo = 'TFCSPE_001'
		WHERE ObjectCode = 'INSERT/09';

		UPDATE Project.tbProject
		SET SubjectCode = 'PALSUP', ContactName = 'Allan Rain', AddressCodeFrom = 'PALSUP_001', AddressCodeTo = 'PALSUP_001', CashCode = NULL, UnitCharge = 0
		WHERE ObjectCode = 'PALLET/01';

		UPDATE Project.tbProject
		SET SubjectCode = 'PLAPRO', ContactName = 'Kim Burnell', AddressCodeFrom = 'PLAPRO_001', AddressCodeTo = 'PLAPRO_001'
		WHERE ObjectCode = 'PC/999';
		
		UPDATE Project.tbProject
		SET SubjectCode = 'HAULOG', ContactName = 'John Iron',  AddressCodeFrom = 'HOME_001', AddressCodeTo = 'STOBOX_001', Quantity = 1, UnitCharge = 25.0 * @ExchangeRate, TotalCharge = 25.0 * @ExchangeRate
		WHERE ObjectCode = 'DELIVERY';

		UPDATE Project.tbProject
		SET SubjectCode = (SELECT SubjectCode FROM App.tbOptions), ContactName = (SELECT UserName FROM Usr.vwCredentials)
		WHERE (CashCode IS NULL) AND (SubjectCode <> 'PALSUP');

		EXEC Project.proc_Schedule @ProjectCode;

		--forward orders
		DECLARE @Month SMALLINT = 1;

		WHILE (@Month < 5)
		BEGIN

			EXEC Project.proc_Copy @FromProjectCode = @ProjectCode, 
					@ToProjectCode = @ToProjectCode OUTPUT;

			UPDATE Project.tbProject
			SET ActionOn = DATEADD(MONTH, @Month, ActionOn)
			WHERE ProjectCode = @ToProjectCode;

			EXEC Project.proc_Schedule @ToProjectCode;

			SET @ProjectCode = @ToProjectCode;
			SET @Month += 1;
		END

		--order the pallets
		EXEC Project.proc_NextCode 'PALLET/01', @ProjectCode OUTPUT
		
		INSERT INTO Project.tbProject
				(ProjectCode, UserId, SubjectCode, ProjectTitle, ObjectCode, ProjectStatusCode, ActionById)
		VALUES        (@ProjectCode,@UserId, 'PALSUP', N'PALLETS', 'PALLET/01', 1, @UserId);

		WITH demand AS
		(
			SELECT ObjectCode, ROUND(SUM(Quantity), -1) AS Quantity, MIN(ActionOn) AS ActionOn
			FROM Project.tbProject project 
			WHERE ObjectCode = 'PALLET/01' AND ProjectCode <> @ProjectCode
			GROUP BY ObjectCode
		)
		UPDATE Project
		SET 
			ProjectNotes = Object.ObjectDescription, 
			Quantity = demand.Quantity,
			ActionOn = demand.ActionOn,
			CashCode = Object.CashCode, 
			TaxCode = Subject.TaxCode, 
			UnitCharge = Object.UnitCharge, 
			AddressCodeFrom = Subject.AddressCode, 
			AddressCodeTo = Subject.AddressCode, 
			Printed = Object.Printed
		FROM Project.tbProject Project
			JOIN Subject.tbSubject Subject ON Project.SubjectCode = Subject.SubjectCode
			JOIN Object.tbObject Object ON Project.ObjectCode = Object.ObjectCode
			JOIN demand ON Project.ObjectCode = demand.ObjectCode
		WHERE ProjectCode = @ProjectCode;

		EXEC Project.proc_Configure @ProjectCode;
		EXEC Project.proc_AssignToParent @ProjectCode, @ParentProjectCode;

		UPDATE Project.tbFlow
		SET StepNumber = 0
		WHERE (ChildProjectCode = @ProjectCode);

		--identify ordered boms
		WITH unique_id AS
		(
			SELECT ProjectCode, ObjectCode, ROW_NUMBER() OVER (PARTITION BY ObjectCode ORDER BY ActionOn) AS RowNo
			FROM Project.tbProject
		)
		UPDATE Project
		SET 
			ProjectTitle = CONCAT(ProjectTitle, ' ', unique_id.RowNo)
		FROM Project.tbProject Project
			JOIN unique_id ON Project.ProjectCode = unique_id.ProjectCode
		WHERE Project.ObjectCode = 'M/00/70/00';

		--borrow some money
		UPDATE Cash.tbCategory
		SET IsEnabled = 1
		WHERE CategoryCode = 'IV';

		UPDATE Cash.tbCode
		SET IsEnabled = 1
		WHERE CashCode = '214';

		DECLARE @PaymentCode NVARCHAR(20), @AccountCode NVARCHAR(10);
		EXEC Cash.proc_CurrentAccount @AccountCode OUTPUT;

		IF @ExchangeRate = 1
		BEGIN
			
			EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT
			INSERT INTO Cash.tbPayment (AccountCode, PaymentCode, UserId, SubjectCode, CashCode, TaxCode, PaidInValue)
			SELECT TOP 1
				@AccountCode AccountCode,
				@PaymentCode AS PaymentCode, 
				@UserId AS UserId,
				SubjectCode,
				'214' AS CashCode,
				'T0' AS TaxCode,
				(SELECT ABS(ROUND(MIN(Balance), -3)) + 1000	FROM Cash.vwStatement) AS PaidInValue
			FROM Subject.tbAccount 
			WHERE AccountCode = @AccountCode

			EXEC Cash.proc_PaymentPost;
		END

		-- ***************************************************************************
		IF @InvoiceOrders = 0
			GOTO CommitTran;
		-- ***************************************************************************
		
		DECLARE 
			@InvoiceTypeCode SMALLINT,
			@InvoiceNumber NVARCHAR(10),
			@InvoicedOn DATETIME = CAST(CURRENT_TIMESTAMP AS DATE);

		DECLARE cur_Projects CURSOR LOCAL FOR
			WITH parent AS
			(
				SELECT DISTINCT FIRST_VALUE(ProjectCode) OVER (PARTITION BY ObjectCode ORDER BY ActionOn) AS ProjectCode
				FROM Project.tbProject Project
				WHERE Project.ObjectCode = 'M/00/70/00'
			), candidates AS
			(
				SELECT child.ParentProjectCode, child.ChildProjectCode
					, 1 AS Depth
				FROM Project.tbFlow child 
					JOIN parent ON child.ParentProjectCode = parent.ProjectCode
					JOIN Project.tbProject Project ON child.ChildProjectCode = Project.ProjectCode

				UNION ALL

				SELECT child.ParentProjectCode, child.ChildProjectCode
					, parent.Depth + 1 AS Depth
				FROM Project.tbFlow child 
					JOIN candidates parent ON child.ParentProjectCode = parent.ChildProjectCode
					JOIN Project.tbProject Project ON child.ChildProjectCode = Project.ProjectCode
			), selected AS
			(
				SELECT ProjectCode
				FROM parent

				UNION

				SELECT ChildProjectCode AS ProjectCode
				FROM candidates

				UNION

				SELECT ProjectCode
				FROM Project.tbProject 
				WHERE ObjectCode = 'PALLET/01'
			)
			SELECT Project.ProjectCode, CASE category.CashPolarityCode WHEN 0 THEN 2 ELSE 0 END AS InvoiceTypeCode
			FROM selected
				JOIN Project.tbProject Project ON selected.ProjectCode = Project.ProjectCode
				JOIN Cash.tbCode cash_code ON Project.CashCode = cash_code.CashCode
				JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode;

		OPEN cur_Projects
		FETCH NEXT FROM cur_Projects INTO @ProjectCode, @InvoiceTypeCode;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @PayInvoices = 0
			BEGIN
				EXEC Invoice.proc_Raise @ProjectCode = @ProjectCode, @InvoiceTypeCode = @InvoiceTypeCode, @InvoicedOn = @InvoicedOn, @InvoiceNumber = @InvoiceNumber OUTPUT
				EXEC Invoice.proc_Accept @InvoiceNumber;
			END
			ELSE
				EXEC Project.proc_Pay @ProjectCode = @ProjectCode, @Post = 1, @PaymentCode = @PaymentCode OUTPUT;

			FETCH NEXT FROM cur_Projects INTO @ProjectCode, @InvoiceTypeCode;
		END

		CLOSE cur_Projects;
		DEALLOCATE cur_Projects;

CommitTran:
			
		COMMIT TRAN;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
