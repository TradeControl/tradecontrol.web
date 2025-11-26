CREATE PROCEDURE [App].[proc_NodeDataInit]
AS
SET NOCOUNT, XACT_ABORT ON;
BEGIN TRY

	BEGIN TRAN
	UPDATE Cash.tbTaxType
	SET SubjectCode = null, CashCode = null;

	DELETE FROM App.tbOptions;
	DELETE FROM dbo.AspNetUsers;
	DELETE FROM Cash.tbPayment;
	DELETE FROM Invoice.tbInvoice;
	DELETE FROM Project.tbFlow;
	DELETE FROM Project.tbProject;
	DELETE FROM Object.tbFlow;
	DELETE FROM Object.tbObject;
	DELETE FROM Subject.tbAccount;
	DELETE FROM Subject.tbSubject;
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
	DELETE FROM App.tbTemplate;
	
	IF NOT EXISTS (SELECT * FROM [Usr].[tbMenuView])
		INSERT INTO [Usr].[tbMenuView] ([MenuViewCode], [MenuView])
		VALUES
	(0, 'List')
	, (1, 'Tree')

	IF NOT EXISTS (SELECT * FROM [Cash].[tbChangeType])
			INSERT INTO [Cash].[tbChangeType] ([ChangeTypeCode], [ChangeType])
			VALUES
		(0, 'Receipt')
		, (1, 'Change')

	IF NOT EXISTS (SELECT * FROM [Cash].[tbChangeStatus])
			INSERT INTO [Cash].[tbChangeStatus] ([ChangeStatusCode], [ChangeStatus])
			VALUES
		(0, 'Unused')
		, (1, 'Paid')
		, (2, 'Spent')

	IF NOT EXISTS (SELECT * FROM [Cash].[tbTxStatus])
			INSERT INTO [Cash].[tbTxStatus] ([TxStatusCode], [TxStatus])
			VALUES
		(0, 'Received')
		, (1, 'UTXO')
		, (2, 'Spent')

	IF NOT EXISTS (SELECT * FROM [Subject].[tbTransmitStatus])
			INSERT INTO [Subject].[tbTransmitStatus] ([TransmitStatusCode], [TransmitStatus])
			VALUES
		(0, 'Disconnected')
		, (1, 'Deploy')
		, (2, 'Update')
		, (3, 'Processed')

	IF NOT EXISTS (SELECT * FROM [App].[tbTemplate])
			INSERT INTO [App].[tbTemplate] ([TemplateName], [StoredProcedure])
			VALUES
		('Basic Company Setup', 'App.proc_TemplateCompanyGeneral')
		, ('HMRC Company Accounts', 'App.proc_TemplateCompanyHMRC2021')
		, ('MIS Tutorials', 'App.proc_TemplateTutorials')

	IF NOT EXISTS (SELECT * FROM [Subject].[tbAccountType])
			INSERT INTO [Subject].[tbAccountType] ([AccountTypeCode], [AccountType])
			VALUES
		(0, 'CASH')
		, (1, 'DUMMY')
		, (2, 'ASSET')

	IF NOT EXISTS (SELECT * FROM [App].[tbUoc])
		INSERT INTO [App].[tbUoc] ([UnitOfCharge], [UocSymbol], [UocName])
		VALUES
			(N'AED', N'د.إ.‏', N'United Arab Emirates Dirhams')
			,(N'ALL', N'Lek', N'Albania Leke')
			,(N'AMD', N'դր.', N'Armenia Drams')
			,(N'ARS', N'$', N'Argentina Pesos')
			,(N'AUD', N'$', N'Australia Dollars')
			,(N'AZM', N'man.', N'Azerbaijan Manats')
			,(N'BGL', N'лв', N'Bulgaria')
			,(N'BHD', N'د.ب.‏', N'Bahrain Dinars')
			,(N'BND', N'$', N'Brunei Dollars')
			,(N'BOB', N'$b', N'Bolivia Bolivianos')
			,(N'BRL', N'R$ ', N'Brazil Reais')
			,(N'BTC', N'₿', N'Bitcoin')
			,(N'BYB', N'р.', N'Belarus')
			,(N'BZD', N'BZ$', N'Belize Dollars')
			,(N'CAD', N'$', N'Canada Dollars')
			,(N'CHF', N'SFr.', N'Switzerland Francs')
			,(N'CLP', N'$', N'Chile Pesos')
			,(N'CNY', N'￥', N'China Yuan Renminbi')
			,(N'COP', N'$', N'Colombia Pesos')
			,(N'CRC', N'₡', N'Costa Rica Colones')
			,(N'CZK', N'Kč', N'Czech Republic Koruny')
			,(N'DKK', N'kr', N'Denmark Kroner')
			,(N'DOP', N'RD$', N'Dominican Republic Pesos')
			,(N'DZD', N'د.ج.‏', N'Algeria Dinars')
			,(N'EEK', N'kr', N'Estonia Krooni')
			,(N'EGP', N'ج.م.‏', N'Egypt Pounds')
			,(N'EUR', N'€', N'Euro')
			,(N'GBP', N'£', N'UK Pounds')
			,(N'GEL', N'Lari', N'Georgia Lari')
			,(N'GTQ', N'Q', N'Guatemala Quetzales')
			,(N'HKD', N'HK$', N'Hong Kong Dollars')
			,(N'HNL', N'L.', N'Honduras Lempiras')
			,(N'HRK', N'kn', N'Croatia Kuna')
			,(N'HUF', N'Ft', N'Hungary Forint')
			,(N'IDR', N'Rp', N'Indonesia Rupiahs')
			,(N'ILS', N'₪', N'Israel New Shekels')
			,(N'INR', N'रु', N'India Rupees')
			,(N'IQD', N'د.ع.‏', N'Iraq Dinars')
			,(N'IRR', N'ريال', N'Iran Rials')
			,(N'ISK', N'kr.', N'Iceland Kronur')
			,(N'JMD', N'J$', N'Jamaica Dollars')
			,(N'JOD', N'د.ا.‏', N'Jordan Dinars')
			,(N'JPY', N'¥', N'Japan Yen')
			,(N'KES', N'S', N'Kenya Shillings')
			,(N'KGS', N'сом', N'Kyrgyzstan Soms')
			,(N'KRW', N'₩', N'South Korea Won')
			,(N'KWD', N'د.ك.‏', N'Kuwait Dinars')
			,(N'KZT', N'Т', N'Kazakhstan Tenge')
			,(N'LBP', N'ل.ل.‏', N'Lebanon Pounds')
			,(N'LTL', N'Lt', N'Lithuania Litai')
			,(N'LVL', N'Ls', N'Latvia Lati')
			,(N'LYD', N'د.ل.‏', N'Libya Dinars')
			,(N'MAD', N'د.م.‏', N'Morocco Dirhams')
			,(N'MKD', N'ден.', N'Macedonia Denars')
			,(N'MNT', N'₮', N'Mongolia Tugriks')
			,(N'MOP', N'P', N'Macau Patacas')
			,(N'MVR', N'ރ.', N'Maldives Rufiyaa')
			,(N'MXN', N'$', N'Mexico Pesos')
			,(N'MYR', N'R', N'Malaysia Ringgits')
			,(N'NIO', N'C$', N'Nicaragua Cordobas')
			,(N'NOK', N'kr', N'Norway Kroner')
			,(N'NZD', N'$', N'New Zealand Dollars')
			,(N'OMR', N'ر.ع.‏', N'Oman Rials')
			,(N'PAB', N'B/.', N'Panama Balboas')
			,(N'PEN', N'S/.', N'Peru Nuevos Soles')
			,(N'PHP', N'Php', N'Philippines Pesos')
			,(N'PKR', N'Rs', N'Pakistan Rupees')
			,(N'PLN', N'zł', N'Poland Zlotych')
			,(N'PYG', N'Gs', N'Paraguay Guarani')
			,(N'QAR', N'ر.ق.‏', N'Qatar Riyals')
			,(N'ROL', N'lei', N'Romania Lei')
			,(N'RUR', N'р.', N'Russia')
			,(N'SAR', N'ر.س.‏', N'Saudi Arabia Riyals')
			,(N'SEK', N'kr', N'Sweden Kronor')
			,(N'SGD', N'$', N'Singapore Dollars')
			,(N'SIT', N'SIT', N'Slovenia Tolars')
			,(N'SKK', N'Sk', N'Slovakia Koruny')
			,(N'SYP', N'ل.س.‏', N'Syria Pounds')
			,(N'THB', N'฿', N'Thailand Baht')
			,(N'TND', N'د.ت.‏', N'Tunisia Dinars')
			,(N'TRL', N'TL', N'Turkey Liras')
			,(N'TTD', N'TT$', N'Trinidad and Tobago Dollars')
			,(N'TWD', N'NT$', N'Taiwan New Dollars')
			,(N'UAH', N'грн.', N'Ukraine Hryvnia')
			,(N'USD', N'$', N'US Dollars')
			,(N'UYU', N'$U', N'Uruguay Pesos')
			,(N'UZS', N'su''m', N'Uzbekistan Sums')
			,(N'VEB', N'Bs', N'Venezuela Bolivares')
			,(N'VND', N'₫', N'Vietnam Dong')
			,(N'YER', N'ر.ي.‏', N'Yemen Rials')
			,(N'YUN', N'Din.', N'Serbia')
			,(N'ZAR', N'R', N'South Africa Rand')
			,(N'ZWD', N'Z$', N'Zimbabwe Dollar');	
	IF NOT EXISTS (SELECT * FROM [App].[tbEventType])
			INSERT INTO [App].[tbEventType] ([EventTypeCode], [EventType])
			VALUES
		(0, 'Error')
		, (1, 'Warning')
		, (2, 'Information')
		, (3, 'Price Change')
		, (4, 'Reschedule')
		, (5, 'Delivered')
		, (6, 'Status Change')
		, (7, 'Payment')
		, (8, 'Pay Address')

	IF NOT EXISTS (SELECT * FROM [Cash].[tbAssetType])
			INSERT INTO [Cash].[tbAssetType] ([AssetTypeCode], [AssetType])
			VALUES
		(0, 'DEBTORS')
		, (1, 'CREDITORS')
		, (2, 'BANK')
		, (3, 'CASH')
		, (4, 'CASH ACCOUNTS')
		, (5, 'CAPITAL')

	IF NOT EXISTS (SELECT * FROM [Cash].[tbPaymentStatus])
			INSERT INTO [Cash].[tbPaymentStatus] ([PaymentStatusCode], [PaymentStatus])
			VALUES
		(0, 'Unposted')
		, (1, 'Posted')
		, (2, 'Transfer')

	IF NOT EXISTS (SELECT * FROM [App].[tbMonth])
			INSERT INTO [App].[tbMonth] ([MonthNumber], [MonthName])
			VALUES
		(1, 'JAN')
		, (2, 'FEB')
		, (3, 'MAR')
		, (4, 'APR')
		, (5, 'MAY')
		, (6, 'JUN')
		, (7, 'JUL')
		, (8, 'AUG')
		, (9, 'SEP')
		, (10, 'OCT')
		, (11, 'NOV')
		, (12, 'DEC')

	IF NOT EXISTS (SELECT * FROM [App].[tbText])
			INSERT INTO [App].[tbText] ([TextId], [Message], [Arguments])
			VALUES
		(1220, 'Invoices deployed to the network cannot be deleted. Add a credit/debit note instead.', 0)
		, (1221, 'Service Log cleared down.', 0)
		, (1222, 'Task Change Log cleared down.', 0)
		, (1223, 'Invoice Change Log cleared down.', 0)
		, (1224, 'Raise corresponding invoices?', 0)
		, (1225, 'Initialising <1>', 1)

	IF NOT EXISTS (SELECT * FROM [Cash].[tbCoinType])
			INSERT INTO [Cash].[tbCoinType] ([CoinTypeCode], [CoinType])
			VALUES
		(0, 'Main')
		, (1, 'TestNet')
		, (2, 'Fiat')

	IF NOT EXISTS (SELECT * FROM [Usr].[tbInterface])
			INSERT INTO [Usr].[tbInterface] ([InterfaceCode], [Interface])
			VALUES
		(0, 'Accounts')
		, (1, 'MIS')


	IF NOT EXISTS(SELECT * FROM App.tbEventType)
		INSERT INTO App.tbEventType (EventTypeCode, EventType)
		VALUES (0, 'Error')
		, (1, 'Warning')
		, (2, 'Information')
		, (3, 'Price Change')
		, (4, 'Reschedule')
		, (5, 'Delivered')
		, (6, 'Status Change')
		, (7, 'Payment')
		, (8, 'Pay Address');

	IF NOT EXISTS(SELECT * FROM App.tbMonth)
		INSERT INTO App.tbMonth (MonthNumber, MonthName)
		VALUES (1, 'JAN')
		, (2, 'FEB')
		, (3, 'MAR')
		, (4, 'APR')
		, (5, 'MAY')
		, (6, 'JUN')
		, (7, 'JUL')
		, (8, 'AUG')
		, (9, 'SEP')
		, (10, 'OCT')
		, (11, 'NOV')
		, (12, 'DEC');

	IF NOT EXISTS(SELECT * FROM Object.tbAttributeType)
		INSERT INTO Object.tbAttributeType (AttributeTypeCode, AttributeType)
		VALUES (0, 'Order')
		, (1, 'Quote');

	IF NOT EXISTS(SELECT * FROM Object.tbSyncType)
		INSERT INTO Object.tbSyncType (SyncTypeCode, SyncType)
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

    IF NOT EXISTS(SELECT * FROM Cash.tbCategoryExpSyntax)
        INSERT INTO Cash.tbCategoryExpSyntax (SyntaxTypeCode, SyntaxType)
        VALUES (0, 'Both')
        , (1, 'Libre')
        , (2, 'Excel');
        
	IF NOT EXISTS(SELECT * FROM Cash.tbEntryType)
		INSERT INTO Cash.tbEntryType (CashEntryTypeCode, CashEntryType)
		VALUES (0, 'Payment')
		, (1, 'Invoice')
		, (2, 'Order')
		, (3, 'Quote')
		, (4, 'Corporation Tax')
		, (5, 'Vat')
		, (6, 'Forecast');

	IF NOT EXISTS(SELECT * FROM Cash.tbPolarity)
		INSERT INTO Cash.tbPolarity (CashPolarityCode, CashPolarity)
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
		INSERT INTO Invoice.tbType (InvoiceTypeCode, InvoiceType, CashPolarityCode, NextNumber)
		VALUES (0, 'Sales Invoice', 1, 10000)
		, (1, 'Credit Note', 0, 20000)
		, (2, 'Purchase Invoice', 0, 30000)
		, (3, 'Debit Note', 1, 40000);

	IF NOT EXISTS (SELECT * FROM Cash.tbPaymentStatus)
		INSERT INTO Cash.tbPaymentStatus (PaymentStatusCode, PaymentStatus)
		VALUES (0, 'Unposted')
		, (1, 'Payment')
		, (2, 'Transfer');

	IF NOT EXISTS(SELECT * FROM Subject.tbStatus)
		INSERT INTO Subject.tbStatus (SubjectStatusCode, SubjectStatus)
		VALUES (0, 'Pending')
		, (1, 'Active')
		, (2, 'Hot')
		, (3, 'Dead');

	IF NOT EXISTS(SELECT * FROM Project.tbOpStatus)
		INSERT INTO Project.tbOpStatus (OpStatusCode, OpStatus)
		VALUES (0, 'Pending')
		, (1, 'In-progress')
		, (2, 'Complete');

	IF NOT EXISTS(SELECT * FROM Project.tbStatus)
		INSERT INTO Project.tbStatus (ProjectStatusCode, ProjectStatus)
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
		VALUES (0, 'Project_QuotationStandard', 2, 'Standard Quotation')
		, (0, 'Project_QuotationTextual', 2, 'Textual Quotation')
		, (1, 'Project_SalesOrder', 2, 'Standard Sales Order')
		, (2, 'Project_PurchaseEnquiryDeliveryStandard', 2, 'Standard Transport Enquiry')
		, (2, 'Project_PurchaseEnquiryDeliveryTextual', 2, 'Textual Transport Enquiry')
		, (2, 'Project_PurchaseEnquiryStandard', 2, 'Standard Purchase Enquiry')
		, (2, 'Project_PurchaseEnquiryTextual', 2, 'Textual Purchase Enquiry')
		, (3, 'Project_PurchaseOrder', 2, 'Standard Purchase Order')
		, (3, 'Project_PurchaseOrderDelivery', 2, 'Purchase Order for Delivery')
		, (4, 'Invoice_Sales', 2, 'Standard Sales Invoice')
		, (4, 'Invoice_SalesLetterhead', 2, 'Sales Invoice for Letterhead Paper')
		, (5, 'Invoice_CreditNote', 2, 'Standard Credit Note')
		, (5, 'Invoice_CreditNoteLetterhead', 2, 'Credit Note for Letterhead Paper')
		, (6, 'Invoice_DebitNote', 2, 'Standard Debit Note')
		, (6, 'Invoice_DebitNoteLetterhead', 2, 'Debit Note for Letterhead Paper');

	IF NOT EXISTS(SELECT * FROM Subject.tbType)
		INSERT INTO Subject.tbType (SubjectTypeCode, CashPolarityCode, SubjectType)
		VALUES (0, 0, 'Supplier')
		, (1, 1, 'Customer')
		, (2, 1, 'Prospect')
		, (4, 1, 'Company')
		, (5, 0, 'Bank')
		, (7, 0, 'Other')
		, (8, 0, 'TBC')
		, (9, 0, 'Employee');

	IF NOT EXISTS(SELECT * FROM Cash.tbCoinType)
		INSERT INTO Cash.tbCoinType (CoinTypeCode, CoinType)
		VALUES (0, 'Main')
		, (1, 'TestNet')
		, (2, 'Fiat');

	IF NOT EXISTS(SELECT * FROM Cash.tbChangeType)
		INSERT INTO Cash.tbChangeType (ChangeTypeCode, ChangeType) 
		VALUES (0, 'Receipt')
		, (1, 'Change');

	IF NOT EXISTS(SELECT * FROM Cash.tbChangeStatus)
		INSERT INTO Cash.tbChangeStatus (ChangeStatusCode, ChangeStatus) 
		VALUES (0, 'Unused')
		, (1, 'Paid')
		, (2, 'Spent');

	IF NOT EXISTS(SELECT * FROM Subject.tbTransmitStatus)
		INSERT INTO Subject.tbTransmitStatus (TransmitStatusCode, TransmitStatus)
		VALUES (0, 'Disconnected')
		, (1, 'Deploy')
		, (2, 'Update')
		, (3, 'Processed');

	IF NOT EXISTS(SELECT * FROM Subject.tbAccountType)
		INSERT INTO Subject.tbAccountType (AccountTypeCode, AccountType)
		VALUES (0, 'CASH'), (1, 'DUMMY'), (2, 'ASSET');

	IF NOT EXISTS(SELECT * FROM Cash.tbAssetType)
		INSERT INTO Cash.tbAssetType (AssetTypeCode, AssetType)
		VALUES (0, 'DEBTORS')
		, (1, 'CREDITORS')
		, (2, 'BANK')
		, (3, 'CASH')
		, (4, 'CASH ACCOUNTS')
		, (5, 'CAPITAL');

	IF NOT EXISTS(SELECT * FROM App.tbTemplate)
		INSERT INTO App.tbTemplate (TemplateName, StoredProcedure)
		VALUES ('Basic Company Setup', 'App.proc_TemplateCompanyGeneral') 
			, ('HMRC Company Accounts', 'App.proc_TemplateCompanyHMRC2021')
			, ('MIS Tutorials', 'App.proc_TemplateTutorials');

	IF NOT EXISTS(SELECT * FROM Usr.tbMenuView)
		INSERT INTO Usr.tbMenuView (MenuViewCode, MenuView)
		VALUES (0, 'List'), (1, 'Tree');

	IF NOT EXISTS(SELECT * FROM Usr.tbInterface)
		INSERT INTO Usr.tbInterface (InterfaceCode, Interface)
		VALUES (0, 'Accounts')
		, (1, 'MIS');

	IF NOT EXISTS(SELECT * FROM App.tbText)
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
		, (3018, 'The balance for this account is zero. Check for unposted payments.', 0)
		, (1220, 'Invoices deployed to the network cannot be deleted. Add a credit/debit note instead.', 0)
		, (1221, 'Service Log cleared down.', 0)
		, (1222, 'Task Change Log cleared down.', 0)
		, (1223, 'Invoice Change Log cleared down.', 0)
		, (1224, 'Raise corresponding invoices?', 0)
		, (1225, 'Initialising <1>', 1)
		;
	END

	COMMIT TRAN
END TRY
BEGIN CATCH
	EXEC App.proc_ErrorLog;
END CATCH
