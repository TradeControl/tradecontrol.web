CREATE PROCEDURE [App].[proc_NodeBusinessInit]
(
	@SubjectCode NVARCHAR(10),
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

	INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode, PhoneNumber, EmailAddress, CompanyNumber, VatNumber)
	VALUES (@SubjectCode, @BusinessName, 4, 1, @PhoneNumber, @BusinessEmailAddress, @CompanyNumber, @VatNumber);

	EXEC Subject.proc_AddContact @SubjectCode = @SubjectCode, @ContactName = @FullName;
	EXEC Subject.proc_AddAddress @SubjectCode = @SubjectCode, @Address = @BusinessAddress;

	INSERT INTO App.tbCalendar (CalendarCode, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday)
	VALUES (@CalendarCode, 1, 1, 1, 1, 1, 0, 0);
		
	INSERT INTO Usr.tbUser (UserId, UserName, LogonName, IsAdministrator, IsEnabled, CalendarCode, EmailAddress, PhoneNumber)
	VALUES (CONCAT(LEFT(@FullName, 1), SUBSTRING(@FullName, CHARINDEX(' ', @FullName) + 1, 1)), @FullName, 
		SUSER_NAME() , 1, 1, @CalendarCode, @UserEmailAddress, @PhoneNumber);

	INSERT INTO App.tbOptions (Identifier, IsInitialised, SubjectCode, RegisterName, DefaultPrintMode, BucketIntervalCode, BucketTypeCode, TaxHorizon, IsAutoOffsetDays, UnitOfCharge)
	VALUES ('TC', 0, @SubjectCode, 'Event Log', 2, 1, 1, 730, 0, @UnitOfCharge);

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
	, (1, 7, 4, 2, 'Cash Account Statements', 4, 'Trader', 'Subject_PaymentAccount', 0)
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
	, (1, 21, 7, 0, 'Subjects', 0, 'Trader', '', 1)
	, (1, 22, 1, 6, 'Subjects', 1, '', '7', 1)
	, (1, 23, 7, 1, 'Subject Maintenance', 4, 'Trader', 'Subject_Maintenance', 0)
	, (1, 24, 7, 2, 'Subject Enquiry', 4, 'Trader', 'Subject_Enquiry', 0)
	, (1, 25, 7, 3, 'Balance Sheet Audit', 5, 'Trader', 'Subject_BalanceSheetAudit', 2)
	, (2, 26, 1, 0, 'MIS', 0, '', 'Root', 0)
	, (2, 27, 2, 0, 'System Settings', 0, 'Trader', '', 0)
	, (2, 28, 2, 1, 'Administration', 4, 'Trader', 'App_Admin', 0)
	, (2, 29, 2, 2, 'SQL Connect', 4, 'Trader', 'Sys_SQLConnect', 0)
	, (2, 30, 2, 5, 'Definitions', 4, 'Trader', 'App_Definition', 0)
	, (2, 31, 4, 0, 'Maintenance', 0, 'Trader', '', 0)
	, (2, 32, 4, 1, 'Subjects', 4, 'Trader', 'Subject_Maintenance', 0)
	, (2, 33, 4, 2, 'Activities', 4, 'Trader', 'Object_Edit', 0)
	, (2, 34, 5, 0, 'Work Flow', 0, 'Trader', '', 0)
	, (2, 35, 5, 1, 'Project Explorer', 4, 'Trader', 'Project_Explorer', 0)
	, (2, 36, 5, 2, 'Document Manager', 4, 'Trader', 'App_DocManager', 0)
	, (2, 37, 5, 3, 'Raise Invoices', 4, 'Trader', 'Invoice_Raise', 0)
	, (2, 38, 6, 0, 'Information', 0, 'Trader', '', 0)
	, (2, 39, 6, 1, 'Subject Enquiry', 2, 'Trader', 'Subject_Enquiry', 0)
	, (2, 40, 6, 2, 'Invoice Register', 4, 'Trader', 'Invoice_Register', 0)
	, (2, 41, 6, 3, 'Cash Statements', 4, 'Trader', 'Subject_PaymentAccount', 0)
	, (2, 42, 6, 4, 'Data Warehouse', 4, 'Trader', 'App_Warehouse', 0)
	, (2, 43, 6, 5, 'Company Statement', 4, 'Trader', 'Cash_Statement', 0)
	, (2, 44, 4, 3, 'Subject Datasheet', 4, 'Trader', 'Subject_Maintenance', 1)
	, (2, 45, 6, 6, 'Job Profit Status by Month', 4, 'Trader', 'Project_ProfitStatus', 0)
	, (2, 46, 5, 6, 'Expenses', 3, 'Trader', 'Project_Expenses', 0)
	, (2, 47, 1, 1, 'System Settings', 1, '', '2', 0)
	, (2, 48, 1, 3, 'Maintenance', 1, '', '4', 0)
	, (2, 49, 1, 4, 'Workflow', 1, '', '5', 0)
	, (2, 50, 1, 5, 'Information', 1, '', '6', 0)
	, (2, 51, 6, 7, 'Status Graphs', 4, 'Trader', 'Cash_StatusGraphs', 0)
	, (2, 53, 4, 4, 'Budget', 4, 'Trader', 'Cash_Budget', 0)
	, (2, 54, 4, 5, 'Assets', 4, 'Trader', 'Cash_Assets', 0)
	, (2, 57, 5, 7, 'Network Allocations', 4, 'Trader', 'Project_Allocation', 0)
	, (2, 58, 5, 8, 'Network Invoices', 4, 'Trader', 'Invoice_Mirror', 0)
	, (2, 62, 7, 0, 'Audit Reports', 0, 'Trader', '', 1)
	, (2, 63, 6, 11, 'Audit Reports', 1, '', '7', 1)
	, (2, 64, 7, 1, 'Corporation Tax Accruals', 5, 'Trader', 'Cash_CorpTaxAuditAccruals', 4)
	, (2, 65, 7, 2, 'Vat Accruals', 5, 'Trader', 'Cash_VatAuditAccruals', 4)
	, (2, 66, 7, 3, 'Balance Sheet Audit', 5, 'Trader', 'Subject_BalanceSheetAudit', 4);
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