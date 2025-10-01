CREATE PROCEDURE App.proc_NodeInitialisation
(
	@AccountCode NVARCHAR(10),
	@BusinessName NVARCHAR(255),
	@FullName NVARCHAR(100),
	@BusinessAddress NVARCHAR(MAX),
	@BusinessEmailAddress NVARCHAR(255) = null,
	@UserEmailAddress NVARCHAR(255) = null,
	@PhoneNumber NVARCHAR(50) = null,
	@CompanyNumber NVARCHAR(20) = null,
	@VatNumber NVARCHAR(20) = null,
	@CalendarCode NVARCHAR(10),
	@UnitOfCharge NVARCHAR(5)
)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		BEGIN TRAN

		UPDATE Cash.tbTaxType
		SET AccountCode = null, CashCode = null;

		DELETE FROM App.tbOptions;

		DELETE FROM Cash.tbPayment;
		DELETE FROM Invoice.tbInvoice;
		DELETE FROM Task.tbFlow;
		DELETE FROM Task.tbTask;
		DELETE FROM Activity.tbFlow;
		DELETE FROM Activity.tbActivity;
		DELETE FROM Org.tbAccount;
		DELETE FROM Org.tbOrg;
		DELETE FROM Usr.tbMenuUser;
		DELETE FROM Usr.tbMenu;
		DELETE FROM Usr.tbUser;
		DELETE FROM App.tbCalendar;

		DELETE FROM App.tbYear;
		DELETE FROM App.tbBucket;
		DELETE FROM App.tbUom;
		DELETE FROM Cash.tbCategoryTotal;
		DELETE FROM Cash.tbCategoryExp;	
		DELETE FROM Cash.tbCode;
		DELETE FROM App.tbTaxCode;
		DELETE FROM Cash.tbTaxType;
		DELETE FROM Cash.tbCategory;
	
		/***************** CONTROL DATA *****************************************/
		IF NOT EXISTS(SELECT * FROM Activity.tbAttributeType)
			INSERT INTO Activity.tbAttributeType (AttributeTypeCode, AttributeType)
			VALUES (0, 'Order')
			, (1, 'Quote');

		IF NOT EXISTS(SELECT * FROM Activity.tbSyncType)
			INSERT INTO Activity.tbSyncType (SyncTypeCode, SyncType)
			VALUES (0, 'SYNC')
			, (1, 'ASYNC')
			, (2, 'CALL-OFF');

		IF NOT EXISTS(SELECT * FROM App.tbBucketInterval)
			INSERT INTO App.tbBucketInterval (BucketIntervalCode, BucketInterval)
			VALUES (0, 'Day')
			, (1, 'Week')
			, (2, 'Month');

		IF NOT EXISTS(SELECT * FROM App.tbBucketType)
			INSERT INTO App.tbBucketType (BucketTypeCode, BucketType)
			VALUES (0, 'Default')
			, (1, 'Sunday')
			, (2, 'Monday')
			, (3, 'Tuesday')
			, (4, 'Wednesday')
			, (5, 'Thursday')
			, (6, 'Friday')
			, (7, 'Saturday')
			, (8, 'Month');

		IF NOT EXISTS(SELECT * FROM App.tbCodeExclusion)
			INSERT INTO App.tbCodeExclusion (ExcludedTag)
			VALUES ('Limited')
			, ('Ltd')
			, ('PLC');

		IF NOT EXISTS(SELECT * FROM App.tbDocClass)
			INSERT INTO App.tbDocClass (DocClassCode, DocClass)
			VALUES (0, 'Product')
			, (1, 'Money');

		IF NOT EXISTS(SELECT * FROM App.tbDocType)
			INSERT INTO App.tbDocType (DocTypeCode, DocType, DocClassCode)
			VALUES (0, 'Quotation', 0)
			, (1, 'Sales Order', 0)
			, (2, 'Enquiry', 0)
			, (3, 'Purchase Order', 0)
			, (4, 'Sales Invoice', 1)
			, (5, 'Credit Note', 1)
			, (6, 'Debit Note', 1);

		IF NOT EXISTS(SELECT * FROM App.tbRecurrence)
			INSERT INTO App.tbRecurrence (RecurrenceCode, Recurrence)
			VALUES (0, 'On Demand')
			, (1, 'Monthly')
			, (2, 'Quarterly')
			, (3, 'Bi-annual')
			, (4, 'Yearly');

		IF NOT EXISTS(SELECT * FROM App.tbRounding)
			INSERT INTO App.tbRounding (RoundingCode, Rounding)
			VALUES (0, 'Round')
			, (1, 'Truncate');


		IF NOT EXISTS(SELECT * FROM Cash.tbCategoryType)
			INSERT INTO Cash.tbCategoryType (CategoryTypeCode, CategoryType)
			VALUES (0, 'Cash Code')
			, (1, 'Total')
			, (2, 'Expression');

		IF NOT EXISTS(SELECT * FROM Cash.tbEntryType)
			INSERT INTO Cash.tbEntryType (CashEntryTypeCode, CashEntryType)
			VALUES (0, 'Payment')
			, (1, 'Invoice')
			, (2, 'Order')
			, (3, 'Quote')
			, (4, 'Corporation Tax')
			, (5, 'Vat')
			, (6, 'Forecast');

		IF NOT EXISTS(SELECT * FROM Cash.tbMode)
			INSERT INTO Cash.tbMode (CashModeCode, CashMode)
			VALUES (0, 'Expense')
			, (1, 'Income')
			, (2, 'Neutral');

		IF NOT EXISTS(SELECT * FROM Cash.tbStatus)
			INSERT INTO Cash.tbStatus (CashStatusCode, CashStatus)
			VALUES (0, 'Forecast')
			, (1, 'Current')
			, (2, 'Closed')
			, (3, 'Archived');

		IF NOT EXISTS(SELECT * FROM Cash.tbTaxType)
			INSERT INTO Cash.tbTaxType (TaxTypeCode, TaxType, MonthNumber, RecurrenceCode, OffsetDays)
			VALUES (0, 'Corporation Tax', 12, 4, 275)
			, (1, 'Vat', 4, 2, 31)
			, (2, 'N.I.', 4, 1, 0)
			, (3, 'General', 4, 0, 0);

		IF NOT EXISTS(SELECT * FROM Cash.tbType)
			INSERT INTO Cash.tbType (CashTypeCode, CashType)
			VALUES (0, 'TRADE')
			, (1, 'EXTERNAL')
			, (2, 'MONEY');

		IF NOT EXISTS(SELECT * FROM Invoice.tbStatus)
			INSERT INTO Invoice.tbStatus (InvoiceStatusCode, InvoiceStatus)
			VALUES (1, 'Invoiced')
			, (2, 'Partially Paid')
			, (3, 'Paid')
			, (0, 'Pending');

		IF NOT EXISTS(SELECT * FROM Invoice.tbType)
			INSERT INTO Invoice.tbType (InvoiceTypeCode, InvoiceType, CashModeCode, NextNumber)
			VALUES (0, 'Sales Invoice', 1, 10000)
			, (1, 'Credit Note', 0, 20000)
			, (2, 'Purchase Invoice', 0, 30000)
			, (3, 'Debit Note', 1, 40000);

		IF NOT EXISTS (SELECT * FROM Cash.tbPaymentStatus)
		BEGIN
			INSERT INTO Cash.tbPaymentStatus (PaymentStatusCode, PaymentStatus)
			VALUES (0, 'Unposted')
			, (1, 'Payment')
			, (2, 'Transfer');
		END

		IF NOT EXISTS(SELECT * FROM Org.tbStatus)
			INSERT INTO Org.tbStatus (OrganisationStatusCode, OrganisationStatus)
			VALUES (0, 'Pending')
			, (1, 'Active')
			, (2, 'Hot')
			, (3, 'Dead');

		IF NOT EXISTS(SELECT * FROM Task.tbOpStatus)
			INSERT INTO Task.tbOpStatus (OpStatusCode, OpStatus)
			VALUES (0, 'Pending')
			, (1, 'In-progress')
			, (2, 'Complete');

		IF NOT EXISTS(SELECT * FROM Task.tbStatus)
			INSERT INTO Task.tbStatus (TaskStatusCode, TaskStatus)
			VALUES (0, 'Pending')
			, (1, 'Open')
			, (2, 'Closed')
			, (3, 'Charged')
			, (4, 'Cancelled')
			, (5, 'Archive');

		IF NOT EXISTS(SELECT * FROM Usr.tbMenuCommand)
			INSERT INTO Usr.tbMenuCommand (Command, CommandText)
			VALUES (0, 'Folder')
			, (1, 'Link')
			, (2, 'Form In Read Mode')
			, (3, 'Form In Add Mode')
			, (4, 'Form In Edit Mode')
			, (5, 'Report');

		IF NOT EXISTS(SELECT * FROM Usr.tbMenuOpenMode) 
			INSERT INTO Usr.tbMenuOpenMode (OpenMode, OpenModeDescription)
			VALUES (0, 'Normal')
			, (1, 'Datasheet')
			, (2, 'Default Printing')
			, (3, 'Direct Printing')
			, (4, 'Print Preview')
			, (5, 'Email RTF')
			, (6, 'Email HTML')
			, (7, 'Email Snapshot')
			, (8, 'Email PDF');

		IF NOT EXISTS(SELECT * FROM App.tbRegister)
			INSERT INTO App.tbRegister (RegisterName, NextNumber)
			VALUES ('Expenses', 40000)
			, ('Event Log', 1)
			, ('Project', 30000)
			, ('Purchase Order', 20000)
			, ('Sales Order', 10000);

		IF NOT EXISTS(SELECT * FROM App.tbDoc)
			INSERT INTO App.tbDoc (DocTypeCode, ReportName, OpenMode, Description)
			VALUES (0, 'Task_QuotationStandard', 2, 'Standard Quotation')
			, (0, 'Task_QuotationTextual', 2, 'Textual Quotation')
			, (1, 'Task_SalesOrder', 2, 'Standard Sales Order')
			, (2, 'Task_PurchaseEnquiryDeliveryStandard', 2, 'Standard Transport Enquiry')
			, (2, 'Task_PurchaseEnquiryDeliveryTextual', 2, 'Textual Transport Enquiry')
			, (2, 'Task_PurchaseEnquiryStandard', 2, 'Standard Purchase Enquiry')
			, (2, 'Task_PurchaseEnquiryTextual', 2, 'Textual Purchase Enquiry')
			, (3, 'Task_PurchaseOrder', 2, 'Standard Purchase Order')
			, (3, 'Task_PurchaseOrderDelivery', 2, 'Purchase Order for Delivery')
			, (4, 'Invoice_Sales', 2, 'Standard Sales Invoice')
			, (4, 'Invoice_SalesLetterhead', 2, 'Sales Invoice for Letterhead Paper')
			, (5, 'Invoice_CreditNote', 2, 'Standard Credit Note')
			, (5, 'Invoice_CreditNoteLetterhead', 2, 'Credit Note for Letterhead Paper')
			, (6, 'Invoice_DebitNote', 2, 'Standard Debit Note')
			, (6, 'Invoice_DebitNoteLetterhead', 2, 'Debit Note for Letterhead Paper');

		IF NOT EXISTS(SELECT * FROM Org.tbType)
			INSERT INTO Org.tbType (OrganisationTypeCode, CashModeCode, OrganisationType)
			VALUES (0, 0, 'Supplier')
			, (1, 1, 'Customer')
			, (2, 1, 'Prospect')
			, (4, 1, 'Company')
			, (5, 0, 'Bank')
			, (7, 0, 'Other')
			, (8, 0, 'TBC')
			, (9, 0, 'Employee');

		IF NOT EXISTS(SELECT * FROM App.tbText WHERE NOT TextId BETWEEN 1220 AND 1225)
		BEGIN
			INSERT INTO App.tbText (TextId, Message, Arguments)
			VALUES (1003, 'Enter new menu name', 0)
			, (1004, 'Team Menu', 0)
			, (1005, 'Ok to delete <1>', 1)
			, (1006, 'Documents cannot be converted into folders. Either delete the document or create a new folder elsewhere on the menu. Press esc key to undo changes.', 0)
			, (1007, '<Menu Item Text>', 0)
			, (1008, 'Documents cannot have other menu items added to them. Please select a folder then try again.', 0)
			, (1009, 'The root cannot be deleted. Please modify the text or remove the menu itself.', 0)
			, (1189, 'Error <1>', 1)
			, (1190, '<1> Source: <2>  (err <3>) <4>', 4)
			, (1192, 'Server error listing:', 0)
			, (1193, 'days', 0)
			, (1194, 'Ok to delete the selected task and all tasks upon which it depends?', 0)
			, (1208, 'A/No: <3>, Ref.: <2>, Title: <4>, Status: <6>. Dear <1>, <5> <7>', 7)
			, (1209, 'Yours sincerely, <1> <2> T: <3> M: <4> W: <5>', 5)
			, (1210, 'Okay to cancel invoice <1>?', 1)
			, (1211, 'Invoice <1> cannot be cancelled because there are payments assigned to it.  Use the debit/credit facility if this account is not properly reconciled.', 1)
			, (1212, 'Invoices are outstanding against account <1>.	By specifying a cash code, invoices will not be matched. Cash codes should only be entered for miscellaneous charges.', 1)
			, (1213, 'Account <1> has no invoices outstanding for this payment and therefore cannot be posted. Please specify a cash code so that one can be automatically generated.', 1)
			, (1214, 'Invoiced', 0)
			, (1215, 'Ordered', 0)
			, (1217, 'Order charge differs from the invoice. Reconcile <1>?', 1)
			, (1218, 'Raise invoice and pay expenses now?', 0)
			, (1219, 'Reserve Balance', 0)
			, (2002, 'Only administrators have access to the system configuration features of this application.', 0)
			, (2003, 'You are not a registered user of this system. Please contact the Administrator if you believe you should have access.', 0)
			, (2004, 'The primary key you have entered contains invalid characters. Digits and letters should be used for these keys. Please amend accordingly or press Esc to cancel.', 0)
			, (2136, 'You have attempted to execute an Application.Run command with an invalid string. The run string is <1>. The error is <2>', 2)
			, (2188, '<1>', 1)
			, (2206, 'Reminder: You are due for a period end close down.  Please follow the relevant procedures to complete this task. Once all financial data has been consolidated, use the Administrator to move onto the next period.', 0)
			, (2312, 'The system is not setup correctly. Make sure you have completed the initialisation procedures then try again.', 0)
			, (3002, 'Periods not generated successfully. Contact support.', 0)
			, (3003, 'Okay to close down the active period? Before proceeding make sure that you have entered and checked your cash details. All invoices and cash transactions will be transferred into the Cash Flow analysis module.', 0)
			, (3004, 'Margin', 0)
			, (3005, 'Opening Balance', 0)
			, (3006, 'Rebuild executed successfully', 0)
			, (3007, 'Ok to rebuild cash accounts? Make sure no transactions are being processed, as this will re-set and update all your invoices.', 0)
			, (3009, 'Charged', 0)
			, (3010, 'Service', 0)
			, (3011, 'Ok to rebuild cash flow history for account <1>? This would normally be required when payments or invoices have been retrospectively revised, or opening balances altered.', 1)
			, (3012, 'Ok to raise an invoice for this task? Use the Invoicing program to create specific invoice types with multiple tasks and additional charges.', 0)
			, (3013, 'Current Balance', 0)
			, (3014, 'This entry cannot be rescheduled', 0)
			, (3015, 'Dummy accounts should not be assigned a cash code', 0)
			, (3016, 'Operations cannot end before they have been started', 0)
			, (3017, 'Cash codes must be of catagory type MONEY', 0)
			, (3018, 'The balance for this account is zero. Check for unposted payments.', 0);
		END

		/***************** BUSINESS DATA *****************************************/

		INSERT INTO Org.tbOrg (AccountCode, AccountName, OrganisationTypeCode, OrganisationStatusCode, PhoneNumber, EmailAddress, CompanyNumber, VatNumber)
		VALUES (@AccountCode, @BusinessName, 4, 1, @PhoneNumber, @BusinessEmailAddress, @CompanyNumber, @VatNumber);

		EXEC Org.proc_AddContact @AccountCode = @AccountCode, @ContactName = @FullName;
		EXEC Org.proc_AddAddress @AccountCode = @AccountCode, @Address = @BusinessAddress;

		INSERT INTO App.tbCalendar (CalendarCode, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday)
		VALUES (@CalendarCode, 1, 1, 1, 1, 1, 0, 0);
		
		INSERT INTO Usr.tbUser (UserId, UserName, LogonName, IsAdministrator, IsEnabled, CalendarCode, EmailAddress, PhoneNumber)
		VALUES (CONCAT(LEFT(@FullName, 1), SUBSTRING(@FullName, CHARINDEX(' ', @FullName) + 1, 1)), @FullName, 
			SUSER_NAME() , 1, 1, @CalendarCode, @UserEmailAddress, @PhoneNumber);

		INSERT INTO App.tbOptions (Identifier, IsInitialised, AccountCode, RegisterName, DefaultPrintMode, BucketIntervalCode, BucketTypeCode, TaxHorizon, IsAutoOffsetDays, UnitOfCharge)
		VALUES ('TC', 0, @AccountCode, 'Event Log', 2, 1, 1, 730, 0, @UnitOfCharge);

		SET IDENTITY_INSERT [Usr].[tbMenu] ON;
		INSERT INTO [Usr].[tbMenu] ([MenuId], [MenuName], [InterfaceCode])
		VALUES (1, 'Accounts', 0)
		, (2, 'MIS', 1);
		SET IDENTITY_INSERT [Usr].[tbMenu] OFF;

		SET IDENTITY_INSERT [Usr].[tbMenuEntry] ON;
		INSERT INTO [Usr].[tbMenuEntry] ([MenuId], [EntryId], [FolderId], [ItemId], [ItemText], [Command], [ProjectName], [Argument], [OpenMode])
		VALUES (1, 1, 1, 0, 'Accounts', 0, '', 'Root', 0)
		, (1, 2, 2, 0, 'System Settings', 0, 'Trader', '', 0)
		, (1, 3, 2, 1, 'Administration', 4, 'Trader', 'App_Admin', 0)
		, (1, 4, 2, 2, 'SQL Connect', 4, 'Trader', 'Sys_SQLConnect', 0)
		, (1, 5, 2, 5, 'Definitions', 4, 'Trader', 'App_Definition', 0)
		, (1, 6, 4, 0, 'Cash Accounts', 0, 'Trader', '', 0)
		, (1, 7, 4, 2, 'Cash Account Statements', 4, 'Trader', 'Org_PaymentAccount', 0)
		, (1, 8, 5, 0, 'Invoicing', 0, 'Trader', '', 0)
		, (1, 9, 5, 3, 'Raise Invoices and Credit Notes', 4, 'Trader', 'Invoice_Entry', 0)
		, (1, 10, 6, 0, 'Transaction Entry', 0, 'Trader', '', 0)
		, (1, 12, 6, 5, 'Asset Entry', 4, 'Trader', 'Cash_Assets', 0)
		, (1, 13, 1, 1, 'System Settings', 1, '', '2', 0)
		, (1, 14, 1, 3, 'Cash Accounts', 1, '', '4', 0)
		, (1, 15, 1, 4, 'Invoicing', 1, '', '5', 0)
		, (1, 16, 1, 5, 'Transaction Entry', 1, '', '6', 0)
		, (1, 17, 5, 5, 'Invoice Register', 4, 'Trader', 'Invoice_Register', 0)
		, (1, 18, 4, 5, 'Bank Transfers', 4, 'Trader', 'Cash_Transfer', 0)
		, (1, 20, 6, 6, 'Budget', 4, 'Trader', 'Cash_Budget', 0)
		, (1, 21, 7, 0, 'Organisations', 0, 'Trader', '', 1)
		, (1, 22, 1, 6, 'Organisations', 1, '', '7', 1)
		, (1, 23, 7, 1, 'Organisation Maintenance', 4, 'Trader', 'Org_Maintenance', 0)
		, (1, 24, 7, 2, 'Organisation Enquiry', 4, 'Trader', 'Org_Enquiry', 0)
		, (1, 25, 7, 3, 'Balance Sheet Audit', 5, 'Trader', 'Org_BalanceSheetAudit', 2)
		, (2, 26, 1, 0, 'MIS', 0, '', 'Root', 0)
		, (2, 27, 2, 0, 'System Settings', 0, 'Trader', '', 0)
		, (2, 28, 2, 1, 'Administration', 4, 'Trader', 'App_Admin', 0)
		, (2, 29, 2, 2, 'SQL Connect', 4, 'Trader', 'Sys_SQLConnect', 0)
		, (2, 30, 2, 5, 'Definitions', 4, 'Trader', 'App_Definition', 0)
		, (2, 31, 4, 0, 'Maintenance', 0, 'Trader', '', 0)
		, (2, 32, 4, 1, 'Organisations', 4, 'Trader', 'Org_Maintenance', 0)
		, (2, 33, 4, 2, 'Activities', 4, 'Trader', 'Activity_Edit', 0)
		, (2, 34, 5, 0, 'Work Flow', 0, 'Trader', '', 0)
		, (2, 35, 5, 1, 'Task Explorer', 4, 'Trader', 'Task_Explorer', 0)
		, (2, 36, 5, 2, 'Document Manager', 4, 'Trader', 'App_DocManager', 0)
		, (2, 37, 5, 3, 'Raise Invoices', 4, 'Trader', 'Invoice_Raise', 0)
		, (2, 38, 6, 0, 'Information', 0, 'Trader', '', 0)
		, (2, 39, 6, 1, 'Organisation Enquiry', 2, 'Trader', 'Org_Enquiry', 0)
		, (2, 40, 6, 2, 'Invoice Register', 4, 'Trader', 'Invoice_Register', 0)
		, (2, 41, 6, 3, 'Cash Statements', 4, 'Trader', 'Org_PaymentAccount', 0)
		, (2, 42, 6, 4, 'Data Warehouse', 4, 'Trader', 'App_Warehouse', 0)
		, (2, 43, 6, 5, 'Company Statement', 4, 'Trader', 'Cash_Statement', 0)
		, (2, 44, 4, 3, 'Organisation Datasheet', 4, 'Trader', 'Org_Maintenance', 1)
		, (2, 45, 6, 6, 'Job Profit Status by Month', 4, 'Trader', 'Task_ProfitStatus', 0)
		, (2, 46, 5, 6, 'Expenses', 3, 'Trader', 'Task_Expenses', 0)
		, (2, 47, 1, 1, 'System Settings', 1, '', '2', 0)
		, (2, 48, 1, 3, 'Maintenance', 1, '', '4', 0)
		, (2, 49, 1, 4, 'Workflow', 1, '', '5', 0)
		, (2, 50, 1, 5, 'Information', 1, '', '6', 0)
		, (2, 51, 6, 7, 'Status Graphs', 4, 'Trader', 'Cash_StatusGraphs', 0)
		, (2, 53, 4, 4, 'Budget', 4, 'Trader', 'Cash_Budget', 0)
		, (2, 54, 4, 5, 'Assets', 4, 'Trader', 'Cash_Assets', 0)
		, (2, 57, 5, 7, 'Network Allocations', 4, 'Trader', 'Task_Allocation', 0)
		, (2, 58, 5, 8, 'Network Invoices', 4, 'Trader', 'Invoice_Mirror', 0)
		, (2, 62, 7, 0, 'Audit Reports', 0, 'Trader', '', 1)
		, (2, 63, 6, 11, 'Audit Reports', 1, '', '7', 1)
		, (2, 64, 7, 1, 'Corporation Tax Accruals', 5, 'Trader', 'Cash_CorpTaxAuditAccruals', 4)
		, (2, 65, 7, 2, 'Vat Accruals', 5, 'Trader', 'Cash_VatAuditAccruals', 4)
		, (2, 66, 7, 3, 'Balance Sheet Audit', 5, 'Trader', 'Org_BalanceSheetAudit', 4);
		SET IDENTITY_INSERT [Usr].[tbMenuEntry] OFF;

		IF @UnitOfCharge <> 'BTC'
		BEGIN
			INSERT INTO Usr.tbMenuEntry (MenuId, FolderId, ItemId, ItemText, Command, ProjectName, Argument, OpenMode)
			VALUES 
				(1, 6, 3, 'Payment Entry', 4, 'Trader', 'Cash_PaymentEntry', 0)
				, (2, 5, 5, 'Transfers', 4, 'Trader', 'Cash_Transfer', 0)
				, (2, 5, 4, 'Payment Entry', 4, 'Trader', 'Cash_PaymentEntry', 0)
				

		END


		INSERT INTO Usr.tbMenuUser (UserId, MenuId)
		SELECT (SELECT UserId FROM Usr.tbUser) AS UserId, MenuId 
		FROM Usr.tbMenu;

		COMMIT TRAN
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
