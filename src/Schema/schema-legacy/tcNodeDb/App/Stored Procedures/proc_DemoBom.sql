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
		DELETE FROM Task.tbFlow;
		DELETE FROM Task.tbTask;
		DELETE FROM Activity.tbFlow;
		DELETE FROM Activity.tbActivity;

		--WITH sys_accounts AS
		--(
		--	SELECT AccountCode FROM App.tbOptions
		--	UNION
		--	SELECT DISTINCT AccountCode FROM Org.tbAccount
		--	UNION
		--	SELECT DISTINCT AccountCode FROM Cash.tbTaxType
		--), candidates AS
		--(
		--	SELECT AccountCode
		--	FROM Org.tbOrg
		--	EXCEPT
		--	SELECT AccountCode 
		--	FROM sys_accounts
		--)
		--DELETE Org.tbOrg 
		--FROM Org.tbOrg JOIN candidates ON Org.tbOrg.AccountCode = candidates.AccountCode;
		
		UPDATE App.tbOptions
		SET IsAutoOffsetDays = 0;

		EXEC App.proc_SystemRebuild;
		--<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

		IF NOT EXISTS( SELECT * FROM App.tbRegister WHERE RegisterName = 'Works Order')
			INSERT INTO App.tbRegister (RegisterName, NextNumber)
			SELECT 'Works Order', (SELECT MAX(NextNumber) + 10000 FROM App.tbRegister) AS NextNumber;

		INSERT INTO Activity.tbActivity (ActivityCode, TaskStatusCode, ActivityDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
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
		INSERT INTO Activity.tbAttribute (ActivityCode, Attribute, PrintOrder, AttributeTypeCode, DefaultText)
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
		INSERT INTO Activity.tbOp (ActivityCode, OperationNumber, SyncTypeCode, Operation, Duration, OffsetDays)
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
		INSERT INTO Activity.tbFlow (ParentCode, StepNumber, ChildCode, SyncTypeCode, OffsetDays, UsedOnQuantity)
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

		IF (NOT EXISTS(SELECT * FROM Org.tbOrg WHERE AccountCode = 'TFCSPE'))
		BEGIN
			INSERT INTO Org.tbOrg (AccountCode, AccountName, OrganisationTypeCode, OrganisationStatusCode, TaxCode, AddressCode, PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance, NumberOfEmployees, CompanyNumber, VatNumber, Turnover, OpeningBalance, EUJurisdiction)
			VALUES 
			  ('PACSER', 'PACKING SERVICES', 8, 1, 'T1', 'PACSER_001', 'EOM', 10, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('PALSUP', 'PALLET SUPPLIER', 8, 1, 'T1', 'PALSUP_001', 'COD', 0, -10, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('PLAPRO', 'PLASTICS PROVIDER', 8, 1, 'T1', 'PLAPRO_001', '30 days from invoice', 15, 30, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('TFCSPE', 'FASTENER SPECIALIST', 8, 1, 'T1', 'TFCSPE_001', 'EOM', 0, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('STOBOX', 'STORAGE BOXES', 1, 1, 'T1', 'STOBOX_001', '60 days from invoice', 5, 60, 0, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			, ('HAULOG', 'HAULIER LOGISTICS', 8, 1, 'T1', 'HAULOG_001', 'EOM', 0, 30, 1, 1, 0, NULL, NULL, 0.0000, 0.0000, 0)
			;
			INSERT INTO Org.tbAddress (AddressCode, AccountCode, Address)
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
			@TaskCode NVARCHAR(20),
			@ParentTaskCode NVARCHAR(20), 
			@ToTaskCode NVARCHAR(20),
			@Quantity DECIMAL(18, 4) = 100;

		EXEC Task.proc_NextCode 'PROJECT', @ParentTaskCode OUTPUT
		INSERT INTO Task.tbTask
								 (TaskCode, UserId, AccountCode, TaskTitle, ActivityCode, TaskStatusCode, ActionById)
		VALUES        (@ParentTaskCode,@UserId, 'STOBOX', N'PIGEON HOLE SHELF ASSEMBLY', N'PROJECT', 0,@UserId)
	
		EXEC Task.proc_NextCode 'M/00/70/00', @TaskCode OUTPUT
		
		INSERT INTO Task.tbTask
				(TaskCode, UserId, AccountCode, TaskTitle, ContactName, ActivityCode, TaskStatusCode, ActionById, TaskNotes, Quantity, CashCode, TaxCode, UnitCharge, AddressCodeFrom, AddressCodeTo, SecondReference, Printed)
		SELECT @TaskCode,@UserId, 'STOBOX', ActivityDescription, 'Francis Brown', ActivityCode, 1,@UserId, ActivityDescription, @Quantity, '103', 'T1', UnitCharge, 'STOBOX_001', 'STOBOX_001', N'12354/2', 0		
		FROM Activity.tbActivity
		WHERE ActivityCode = 'M/00/70/00';

		EXEC Task.proc_Configure @TaskCode;
		EXEC Task.proc_AssignToParent @TaskCode, @ParentTaskCode;

	
		UPDATE Task.tbTask
		SET AccountCode = 'PACSER', ContactName = 'John OGroats', AddressCodeFrom = 'PACSER_001', AddressCodeTo = 'PACSER_001'
		WHERE ActivityCode = 'BOX/41';

		UPDATE Task.tbTask
		SET AccountCode = 'TFCSPE', ContactName = 'Gary Granger', AddressCodeFrom = 'TFCSPE_001', AddressCodeTo = 'TFCSPE_001'
		WHERE ActivityCode = 'INSERT/09';

		UPDATE Task.tbTask
		SET AccountCode = 'PALSUP', ContactName = 'Allan Rain', AddressCodeFrom = 'PALSUP_001', AddressCodeTo = 'PALSUP_001', CashCode = NULL, UnitCharge = 0
		WHERE ActivityCode = 'PALLET/01';

		UPDATE Task.tbTask
		SET AccountCode = 'PLAPRO', ContactName = 'Kim Burnell', AddressCodeFrom = 'PLAPRO_001', AddressCodeTo = 'PLAPRO_001'
		WHERE ActivityCode = 'PC/999';
		
		UPDATE Task.tbTask
		SET AccountCode = 'HAULOG', ContactName = 'John Iron',  AddressCodeFrom = 'HOME_001', AddressCodeTo = 'STOBOX_001', Quantity = 1, UnitCharge = 25.0 * @ExchangeRate, TotalCharge = 25.0 * @ExchangeRate
		WHERE ActivityCode = 'DELIVERY';

		UPDATE Task.tbTask
		SET AccountCode = (SELECT AccountCode FROM App.tbOptions), ContactName = (SELECT UserName FROM Usr.vwCredentials)
		WHERE (CashCode IS NULL) AND (AccountCode <> 'PALSUP');

		EXEC Task.proc_Schedule @TaskCode;

		--forward orders
		DECLARE @Month SMALLINT = 1;

		WHILE (@Month < 5)
		BEGIN

			EXEC Task.proc_Copy @FromTaskCode = @TaskCode, 
					@ToTaskCode = @ToTaskCode OUTPUT;

			UPDATE Task.tbTask
			SET ActionOn = DATEADD(MONTH, @Month, ActionOn)
			WHERE TaskCode = @ToTaskCode;

			EXEC Task.proc_Schedule @ToTaskCode;

			SET @TaskCode = @ToTaskCode;
			SET @Month += 1;
		END

		--order the pallets
		EXEC Task.proc_NextCode 'PALLET/01', @TaskCode OUTPUT
		
		INSERT INTO Task.tbTask
				(TaskCode, UserId, AccountCode, TaskTitle, ActivityCode, TaskStatusCode, ActionById)
		VALUES        (@TaskCode,@UserId, 'PALSUP', N'PALLETS', 'PALLET/01', 1, @UserId);

		WITH demand AS
		(
			SELECT ActivityCode, ROUND(SUM(Quantity), -1) AS Quantity, MIN(ActionOn) AS ActionOn
			FROM Task.tbTask project 
			WHERE ActivityCode = 'PALLET/01' AND TaskCode <> @TaskCode
			GROUP BY ActivityCode
		)
		UPDATE task
		SET 
			TaskNotes = activity.ActivityDescription, 
			Quantity = demand.Quantity,
			ActionOn = demand.ActionOn,
			CashCode = activity.CashCode, 
			TaxCode = org.TaxCode, 
			UnitCharge = activity.UnitCharge, 
			AddressCodeFrom = org.AddressCode, 
			AddressCodeTo = org.AddressCode, 
			Printed = activity.Printed
		FROM Task.tbTask task
			JOIN Org.tbOrg org ON task.AccountCode = org.AccountCode
			JOIN Activity.tbActivity activity ON task.ActivityCode = activity.ActivityCode
			JOIN demand ON task.ActivityCode = demand.ActivityCode
		WHERE TaskCode = @TaskCode;

		EXEC Task.proc_Configure @TaskCode;
		EXEC Task.proc_AssignToParent @TaskCode, @ParentTaskCode;

		UPDATE Task.tbFlow
		SET StepNumber = 0
		WHERE (ChildTaskCode = @TaskCode);

		--identify ordered boms
		WITH unique_id AS
		(
			SELECT TaskCode, ActivityCode, ROW_NUMBER() OVER (PARTITION BY ActivityCode ORDER BY ActionOn) AS RowNo
			FROM Task.tbTask
		)
		UPDATE task
		SET 
			TaskTitle = CONCAT(TaskTitle, ' ', unique_id.RowNo)
		FROM Task.tbTask task
			JOIN unique_id ON task.TaskCode = unique_id.TaskCode
		WHERE task.ActivityCode = 'M/00/70/00';

		--borrow some money
		UPDATE Cash.tbCategory
		SET IsEnabled = 1
		WHERE CategoryCode = 'IV';

		UPDATE Cash.tbCode
		SET IsEnabled = 1
		WHERE CashCode = '214';

		DECLARE @PaymentCode NVARCHAR(20), @CashAccountCode NVARCHAR(10);
		EXEC Cash.proc_CurrentAccount @CashAccountCode OUTPUT;

		IF @ExchangeRate = 1
		BEGIN
			
			EXEC Cash.proc_NextPaymentCode @PaymentCode OUTPUT
			INSERT INTO Cash.tbPayment (CashAccountCode, PaymentCode, UserId, AccountCode, CashCode, TaxCode, PaidInValue)
			SELECT TOP 1
				@CashAccountCode CashAccountCode,
				@PaymentCode AS PaymentCode, 
				@UserId AS UserId,
				AccountCode,
				'214' AS CashCode,
				'T0' AS TaxCode,
				(SELECT ABS(ROUND(MIN(Balance), -3)) + 1000	FROM Cash.vwStatement) AS PaidInValue
			FROM Org.tbAccount 
			WHERE CashAccountCode = @CashAccountCode

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

		DECLARE cur_tasks CURSOR LOCAL FOR
			WITH parent AS
			(
				SELECT DISTINCT FIRST_VALUE(TaskCode) OVER (PARTITION BY ActivityCode ORDER BY ActionOn) AS TaskCode
				FROM Task.tbTask task
				WHERE task.ActivityCode = 'M/00/70/00'
			), candidates AS
			(
				SELECT child.ParentTaskCode, child.ChildTaskCode
					, 1 AS Depth
				FROM Task.tbFlow child 
					JOIN parent ON child.ParentTaskCode = parent.TaskCode
					JOIN Task.tbTask task ON child.ChildTaskCode = task.TaskCode

				UNION ALL

				SELECT child.ParentTaskCode, child.ChildTaskCode
					, parent.Depth + 1 AS Depth
				FROM Task.tbFlow child 
					JOIN candidates parent ON child.ParentTaskCode = parent.ChildTaskCode
					JOIN Task.tbTask task ON child.ChildTaskCode = task.TaskCode
			), selected AS
			(
				SELECT TaskCode
				FROM parent

				UNION

				SELECT ChildTaskCode AS TaskCode
				FROM candidates

				UNION

				SELECT TaskCode
				FROM Task.tbTask 
				WHERE ActivityCode = 'PALLET/01'
			)
			SELECT task.TaskCode, CASE category.CashModeCode WHEN 0 THEN 2 ELSE 0 END AS InvoiceTypeCode
			FROM selected
				JOIN Task.tbTask task ON selected.TaskCode = task.TaskCode
				JOIN Cash.tbCode cash_code ON task.CashCode = cash_code.CashCode
				JOIN Cash.tbCategory category ON cash_code.CategoryCode = category.CategoryCode;

		OPEN cur_tasks
		FETCH NEXT FROM cur_tasks INTO @TaskCode, @InvoiceTypeCode;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF @PayInvoices = 0
			BEGIN
				EXEC Invoice.proc_Raise @TaskCode = @TaskCode, @InvoiceTypeCode = @InvoiceTypeCode, @InvoicedOn = @InvoicedOn, @InvoiceNumber = @InvoiceNumber OUTPUT
				EXEC Invoice.proc_Accept @InvoiceNumber;
			END
			ELSE
				EXEC Task.proc_Pay @TaskCode = @TaskCode, @Post = 1, @PaymentCode = @PaymentCode OUTPUT;

			FETCH NEXT FROM cur_tasks INTO @TaskCode, @InvoiceTypeCode;
		END

		CLOSE cur_tasks;
		DEALLOCATE cur_tasks;

CommitTran:
			
		COMMIT TRAN;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
