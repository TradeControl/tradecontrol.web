/**************************************************************************************
Trade Control
Upgrade script for ASP.NET Core interface
Requires release 3.34.4 or above

Date: 14 May 2021
Author: IAM

Trade Control by Trade Control Ltd is licensed under GNU General Public License v3.0. 

You may obtain a copy of the License at

	https://www.gnu.org/licenses/gpl-3.0.en.html


***********************************************************************************/
CREATE OR ALTER PROCEDURE Cash.proc_PaymentPostById(@UserId nvarchar(10)) 
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @PaymentCode nvarchar(20)

		DECLARE curMisc cursor local for
			SELECT        Cash.tbPayment.PaymentCode
			FROM            Cash.tbPayment 
				INNER JOIN Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode 
				INNER JOIN Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
				INNER JOIN Org.tbAccount ON Org.tbAccount.CashAccountCode = Cash.tbPayment.CashAccountCode
			WHERE (Org.tbAccount.AccountTypeCode < 2)
				AND (Cash.tbPayment.PaymentStatusCode = 0) 
				AND (Cash.tbPayment.UserId = @UserId)

			ORDER BY Cash.tbPayment.AccountCode, Cash.tbPayment.PaidOn

		DECLARE curInv cursor local for
			SELECT     PaymentCode
			FROM         Cash.tbPayment
			WHERE     (PaymentStatusCode = 0) AND (CashCode IS NULL)
				AND (Cash.tbPayment.UserId = @UserId)
			ORDER BY AccountCode, PaidOn
		
		BEGIN TRANSACTION

		OPEN curMisc
		FETCH NEXT FROM curMisc INTO @PaymentCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Cash.proc_PaymentPostMisc @PaymentCode		
			FETCH NEXT FROM curMisc INTO @PaymentCode	
			END

		CLOSE curMisc
		DEALLOCATE curMisc
	
		OPEN curInv
		FETCH NEXT FROM curInv INTO @PaymentCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Cash.proc_PaymentPostInvoiced @PaymentCode		
			FETCH NEXT FROM curInv INTO @PaymentCode	
			END

		CLOSE curInv
		DEALLOCATE curInv

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

go
ALTER PROCEDURE Cash.proc_PaymentPost
AS
	DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
	EXECUTE Cash.proc_PaymentPostById @UserId;
go
CREATE OR ALTER VIEW Org.vwCashAccounts
	AS
	SELECT        Org.tbAccount.CashAccountCode, Org.tbOrg.AccountCode, Org.tbAccount.CashAccountName, Org.tbOrg.AccountName, Org.tbType.OrganisationType, Org.tbAccount.OpeningBalance, Org.tbAccount.CurrentBalance, 
							 Org.tbAccount.SortCode, Org.tbAccount.AccountNumber, Org.tbAccount.AccountClosed, Org.tbAccountType.AccountType
	FROM            Org.tbOrg INNER JOIN
							 Org.tbAccount ON Org.tbOrg.AccountCode = Org.tbAccount.AccountCode INNER JOIN
							 Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
							 Org.tbAccountType ON Org.tbAccount.AccountTypeCode = Org.tbAccountType.AccountTypeCode;
go
IF EXISTS(SELECT * FROM sys.indexes WHERE [name] = 'IX_Org_tbAccount_CashAccountName')
	DROP INDEX IX_Org_tbAccount_CashAccountName ON Org.tbAccount;
go
CREATE UNIQUE NONCLUSTERED INDEX IX_Org_tbAccount_CashAccountName ON Org.tbAccount (CashAccountName ASC);
go
ALTER VIEW Cash.vwTransferCodeLookup
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCategory.Category, Cash.tbMode.CashMode, Cash.tbMode.CashModeCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbMode ON Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode LEFT OUTER JOIN
							 Org.tbAccount ON Cash.tbCode.CashCode = Org.tbAccount.CashCode
	WHERE        (Cash.tbCode.IsEnabled <> 0) AND (Cash.tbCategory.IsEnabled <> 0) AND (Cash.tbCategory.CashTypeCode = 2) AND (Cash.tbMode.CashModeCode < 2) AND (Org.tbAccount.CashAccountCode IS NULL)
go
ALTER VIEW Org.vwCashAccounts
AS
	SELECT        Org.tbAccount.CashAccountCode, Org.tbOrg.AccountCode, Org.tbAccount.CashAccountName, Org.tbOrg.AccountName, Org.tbType.OrganisationType, Org.tbAccount.OpeningBalance, Org.tbAccount.CurrentBalance, 
							 Org.tbAccount.SortCode, Org.tbAccount.AccountNumber, Org.tbAccount.AccountClosed, Org.tbAccountType.AccountType, Org.tbAccount.CashCode, Cash.tbCode.CashDescription, Org.tbAccount.InsertedBy, 
							 Org.tbAccount.InsertedOn, Org.tbAccount.LiquidityLevel
	FROM            Org.tbOrg INNER JOIN
							 Org.tbAccount ON Org.tbOrg.AccountCode = Org.tbAccount.AccountCode INNER JOIN
							 Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
							 Org.tbAccountType ON Org.tbAccount.AccountTypeCode = Org.tbAccountType.AccountTypeCode LEFT OUTER JOIN
							 Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode AND Org.tbAccount.CashCode = Cash.tbCode.CashCode AND Org.tbAccount.CashCode = Cash.tbCode.CashCode
go
ALTER VIEW Cash.vwBankCashCodes
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCode.TaxCode, Cash.tbCategory.CashModeCode
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode LEFT OUTER JOIN
							 Cash.vwTransferCodeLookup ON Cash.tbCode.CashCode = Cash.vwTransferCodeLookup.CashCode
	WHERE        (Cash.tbCategory.CashTypeCode = 2) AND (Cash.vwTransferCodeLookup.CashCode IS NULL)
go
ALTER VIEW Org.vwCashAccountAssets
AS
	SELECT        Org.tbAccount.CashAccountCode, Org.tbAccount.LiquidityLevel, Org.tbAccount.CashAccountName, Org.tbAccount.AccountCode, Cash.tbCode.CashCode, Cash.tbCode.TaxCode, Org.tbAccount.AccountClosed
	FROM            Org.tbAccount INNER JOIN
							 Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode
	WHERE        (Org.tbAccount.AccountTypeCode = 2);
go
CREATE OR ALTER VIEW Org.vwAddressList
AS
	SELECT        Org.tbOrg.AccountCode, Org.tbAddress.AddressCode, Org.tbOrg.AccountName, Org.tbStatus.OrganisationStatusCode, Org.tbStatus.OrganisationStatus, Org.tbType.OrganisationTypeCode, Org.tbType.OrganisationType, 
							 Org.tbAddress.Address, Org.tbAddress.InsertedBy, Org.tbAddress.InsertedOn, CAST(CASE WHEN Org.tbAddress.AddressCode = Org.tbOrg.AddressCode THEN 1 ELSE 0 END AS bit) AS IsAdminAddress
	FROM            Org.tbOrg INNER JOIN
							 Org.tbAddress ON Org.tbOrg.AccountCode = Org.tbAddress.AccountCode INNER JOIN
							 Org.tbStatus ON Org.tbOrg.OrganisationStatusCode = Org.tbStatus.OrganisationStatusCode INNER JOIN
							 Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode
go
CREATE OR ALTER VIEW Org.vwAccountLookupAll
AS
	SELECT Org.tbOrg.AccountCode, Org.tbOrg.AccountName, Org.tbOrg.OrganisationTypeCode, Org.tbType.OrganisationType, Cash.tbMode.CashMode, Cash.tbMode.CashModeCode, Org.tbOrg.OrganisationStatusCode, Org.tbStatus.OrganisationStatus
	FROM Org.tbOrg 
		JOIN Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode
		JOIN Cash.tbMode ON Org.tbType.CashModeCode = Cash.tbMode.CashModeCode 
		JOIN Org.tbStatus ON Org.tbOrg.OrganisationStatusCode = Org.tbStatus.OrganisationStatusCode;

go
ALTER VIEW Org.vwContacts
AS
	WITH ContactCount AS 
	(
		SELECT ContactName, COUNT(TaskCode) AS Tasks
        FROM Task.tbTask
        WHERE (TaskStatusCode < 2)
        GROUP BY ContactName
        HAVING (ContactName IS NOT NULL)
	)
    SELECT Org.tbContact.ContactName, Org.tbOrg.AccountCode, COALESCE(ContactCount.Tasks, 0) Tasks, Org.tbContact.PhoneNumber, Org.tbContact.HomeNumber, Org.tbContact.MobileNumber,  
                              Org.tbContact.EmailAddress, Org.tbOrg.AccountName, Org.tbType.OrganisationType, Org.tbStatus.OrganisationStatus, Org.tbContact.NameTitle, Org.tbContact.NickName, Org.tbContact.JobTitle, 
                              Org.tbContact.Department, Org.tbContact.Information, Org.tbContact.InsertedBy, Org.tbContact.InsertedOn
     FROM            Org.tbOrg INNER JOIN
                              Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
                              Org.tbStatus ON Org.tbOrg.OrganisationStatusCode = Org.tbStatus.OrganisationStatusCode INNER JOIN
                              Org.tbContact ON Org.tbOrg.AccountCode = Org.tbContact.AccountCode LEFT OUTER JOIN
                              ContactCount ON Org.tbContact.ContactName = ContactCount.ContactName
     WHERE        (Org.tbOrg.OrganisationStatusCode < 3);
go
CREATE OR ALTER VIEW Invoice.vwEntry
AS
	SELECT        Invoice.tbEntry.UserId, Usr.tbUser.UserName, Invoice.tbEntry.AccountCode, Org.tbOrg.AccountName, Invoice.tbEntry.CashCode, Cash.tbCode.CashDescription, Invoice.tbEntry.InvoiceTypeCode, Invoice.tbType.InvoiceType, 
							 Invoice.tbEntry.InvoicedOn, Invoice.tbEntry.TaxCode, App.tbTaxCode.TaxDescription, Cash.tbTaxType.TaxType, Invoice.tbEntry.ItemReference, Invoice.tbEntry.TotalValue, Invoice.tbEntry.InvoiceValue, 
							 Invoice.tbEntry.InvoiceValue + Invoice.tbEntry.TotalValue AS EntryValue
	FROM            Invoice.tbEntry INNER JOIN
							 Org.tbOrg ON Invoice.tbEntry.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 Cash.tbCode ON Invoice.tbEntry.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Invoice.tbType ON Invoice.tbEntry.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Usr.tbUser ON Invoice.tbEntry.UserId = Usr.tbUser.UserId INNER JOIN
							 App.tbTaxCode ON Invoice.tbEntry.TaxCode = App.tbTaxCode.TaxCode INNER JOIN
							 Cash.tbTaxType ON App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND 
							 App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode AND App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode
go
CREATE OR ALTER PROCEDURE Invoice.proc_PostEntriesById(@UserId nvarchar(10))
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@AccountCode nvarchar(10)
			, @InvoiceTypeCode smallint
			, @InvoiceNumber nvarchar(20);
			
		DECLARE c1 CURSOR LOCAL FOR
			SELECT AccountCode, InvoiceTypeCode
			FROM Invoice.tbEntry
			WHERE UserId = @UserId
			GROUP BY AccountCode, InvoiceTypeCode;

		OPEN c1;

		BEGIN TRAN;

		FETCH NEXT FROM c1 INTO @AccountCode, @InvoiceTypeCode;
		
		WHILE (@@FETCH_STATUS = 0)
		BEGIN
			EXEC Invoice.proc_RaiseBlank @AccountCode, @InvoiceTypeCode, @InvoiceNumber output;

			WITH invoice_entry AS
			(
				SELECT @InvoiceNumber InvoiceNumber, MIN(InvoicedOn) InvoicedOn
				FROM Invoice.tbEntry
				WHERE AccountCode = @AccountCode AND InvoiceTypeCode = @InvoiceTypeCode
			)
			UPDATE Invoice.tbInvoice
			SET 
				UserId = @UserId,
				InvoicedOn = invoice_entry.InvoicedOn,
				Printed = CASE WHEN  @InvoiceTypeCode < 2 THEN 0 ELSE 1 END
			FROM Invoice.tbInvoice invoice_header 
				JOIN invoice_entry ON invoice_header.InvoiceNumber = invoice_entry.InvoiceNumber;

			INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue)
			SELECT @InvoiceNumber InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue, InvoiceValue
			FROM Invoice.tbEntry
			WHERE AccountCode = @AccountCode AND InvoiceTypeCode = @InvoiceTypeCode

			EXEC Invoice.proc_Accept @InvoiceNumber;

			FETCH NEXT FROM c1 INTO @AccountCode, @InvoiceTypeCode;
		END

		DELETE FROM Invoice.tbEntry
		WHERE UserId = @UserId;

		COMMIT TRAN;

		CLOSE c1;
		DEALLOCATE c1;

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
CREATE OR ALTER PROCEDURE Invoice.proc_PostEntries
AS
	DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
	EXECUTE Invoice.proc_PostEntriesById @UserId;
go
ALTER VIEW Invoice.vwRegisterTasks
AS
	SELECT (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
			Invoice.tbInvoice.InvoiceNumber, InvoiceTask.TaskCode, Task.TaskTitle, Cash.tbCode.CashCode, Cash.tbCode.CashDescription, 
							 InvoiceTask.TaxCode, App.tbTaxCode.TaxDescription, Invoice.tbInvoice.AccountCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
							 Invoice.tbInvoice.InvoicedOn,  Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, InvoiceTask.Quantity,
							 CASE WHEN Invoice.tbType.CashModeCode = 0 THEN InvoiceTask.InvoiceValue * - 1 ELSE InvoiceTask.InvoiceValue END AS InvoiceValue, 
							 CASE WHEN Invoice.tbType.CashModeCode = 0 THEN InvoiceTask.TaxValue * - 1 ELSE InvoiceTask.TaxValue END AS TaxValue, 
							 Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Printed, 
							 Org.tbOrg.AccountName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashModeCode, Invoice.tbType.InvoiceType
	FROM            Invoice.tbInvoice INNER JOIN
							 Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
							 Invoice.tbTask AS InvoiceTask ON Invoice.tbInvoice.InvoiceNumber = InvoiceTask.InvoiceNumber INNER JOIN
							 Cash.tbCode ON InvoiceTask.CashCode = Cash.tbCode.CashCode INNER JOIN
							 Task.tbTask AS Task ON InvoiceTask.TaskCode = Task.TaskCode AND InvoiceTask.TaskCode = Task.TaskCode LEFT OUTER JOIN
							 App.tbTaxCode ON InvoiceTask.TaxCode = App.tbTaxCode.TaxCode;
go
ALTER VIEW Invoice.vwRegisterItems
AS
	SELECT       (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
					Invoice.tbInvoice.InvoiceNumber, Invoice.tbItem.CashCode AS TaskCode, Cash.tbCode.CashCode, Cash.tbCode.CashDescription, 
							 Invoice.tbItem.TaxCode, App.tbTaxCode.TaxDescription, Invoice.tbInvoice.AccountCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
							 Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn,
							 CASE WHEN Invoice.tbType.CashModeCode = 0 THEN Invoice.tbItem.InvoiceValue * - 1 ELSE Invoice.tbItem.InvoiceValue END AS InvoiceValue, 
							 CASE WHEN Invoice.tbType.CashModeCode = 0 THEN Invoice.tbItem.TaxValue * - 1 ELSE Invoice.tbItem.TaxValue END AS TaxValue, 
							 CAST(Invoice.tbItem.ItemReference as nvarchar(100)) ItemReference, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Printed, 
							 Org.tbOrg.AccountName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashModeCode, Invoice.tbType.InvoiceType
	FROM            Invoice.tbInvoice INNER JOIN
							 Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId INNER JOIN
							 Invoice.tbItem ON Invoice.tbInvoice.InvoiceNumber = Invoice.tbItem.InvoiceNumber INNER JOIN
							 Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							 App.tbTaxCode ON Invoice.tbItem.TaxCode = App.tbTaxCode.TaxCode;
go
ALTER VIEW Invoice.vwRegisterDetail
AS
	WITH register AS
	(
		SELECT     StartOn, InvoiceNumber, TaskCode, CashCode, CashDescription, TaxCode, TaxDescription, AccountCode, InvoiceTypeCode, InvoiceStatusCode, 
							  InvoicedOn, DueOn, ExpectedOn, CAST(Quantity as float) Quantity, CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, PaymentTerms, Printed, AccountName, UserName, UserId, InvoiceStatus, CashModeCode, 
							  InvoiceType, CAST(1 as bit) IsTask, NULL ItemReference
		FROM         Invoice.vwRegisterTasks
		UNION
		SELECT     StartOn, InvoiceNumber, TaskCode, CashCode, CashDescription, TaxCode, TaxDescription, AccountCode, InvoiceTypeCode, InvoiceStatusCode, 
							  InvoicedOn, DueOn, ExpectedOn, CAST(0 as float) Quantity, CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, PaymentTerms, Printed, AccountName, UserName, UserId, InvoiceStatus, CashModeCode, 
							  InvoiceType, CAST(0 as bit) IsTask, ItemReference
		FROM         Invoice.vwRegisterItems
	)
	SELECT StartOn, InvoiceNumber, TaskCode, CashCode, CashDescription, TaxCode, TaxDescription, AccountCode, InvoiceTypeCode, InvoiceStatusCode, 
		InvoicedOn, DueOn, ExpectedOn, PaymentTerms, Printed, AccountName, UserName, UserId, InvoiceStatus, CashModeCode, InvoiceType,
		Quantity, InvoiceValue, TaxValue, (InvoiceValue + TaxValue) TotalValue, IsTask, ItemReference
	FROM register;
go
ALTER VIEW Invoice.vwRegister
AS
	WITH register AS 
	(
		SELECT       (SELECT TOP (1) p.StartOn FROM App.tbYearPeriod p WHERE (p.StartOn <= Invoice.tbInvoice.InvoicedOn) ORDER BY p.StartOn DESC) AS StartOn,  
				Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.AccountCode, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, 
								 Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, CASE WHEN Invoice.tbType.CashModeCode = 0 THEN Invoice.tbInvoice.InvoiceValue * - 1 ELSE Invoice.tbInvoice.InvoiceValue END AS InvoiceValue, 
								 CASE WHEN Invoice.tbType.CashModeCode = 0 THEN Invoice.tbInvoice.TaxValue * - 1 ELSE Invoice.tbInvoice.TaxValue END AS TaxValue, 
								 CASE WHEN Invoice.tbType.CashModeCode = 0 THEN Invoice.tbInvoice.PaidValue * - 1 ELSE Invoice.tbInvoice.PaidValue END AS PaidValue, 
								 CASE WHEN Invoice.tbType.CashModeCode = 0 THEN Invoice.tbInvoice.PaidTaxValue * - 1 ELSE Invoice.tbInvoice.PaidTaxValue END AS PaidTaxValue, Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes, 
								 Invoice.tbInvoice.Printed, Org.tbOrg.AccountName, Usr.tbUser.UserName, Invoice.tbInvoice.UserId, Invoice.tbStatus.InvoiceStatus, Invoice.tbType.CashModeCode, Invoice.tbType.InvoiceType
		FROM            Invoice.tbInvoice INNER JOIN
								 Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode INNER JOIN
								 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
								 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
								 Usr.tbUser ON Invoice.tbInvoice.UserId = Usr.tbUser.UserId
		WHERE        (Invoice.tbInvoice.AccountCode <> (SELECT AccountCode FROM App.tbOptions))
	)
	SELECT StartOn, InvoiceNumber, AccountCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, DueOn, ExpectedOn,
		CAST(InvoiceValue as float) InvoiceValue, CAST(TaxValue as float) TaxValue, CAST((InvoiceValue + TaxValue) as float) TotalInvoiceValue, 
		CAST(PaidValue as float) PaidValue, CAST(PaidTaxValue as float) PaidTaxValue, CAST((PaidValue + PaidTaxValue) as float) TotalPaidValue,
		PaymentTerms, Notes, Printed, AccountName, UserName, UserId, InvoiceStatus, CashModeCode, InvoiceType
	FROM register;
go
DROP VIEW Invoice.vwRegisterSales
go
CREATE OR ALTER  VIEW Invoice.vwRegisterSales
AS
SELECT        StartOn, InvoiceNumber, AccountCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, PaymentTerms, Notes, Printed, AccountName, UserName, 
                         InvoiceStatus, CashModeCode, InvoiceType, (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) AS UnpaidValue
FROM            Invoice.vwRegister
WHERE        (InvoiceTypeCode < 2);
go
DROP VIEW Invoice.vwRegisterPurchases
go
CREATE OR ALTER VIEW Invoice.vwRegisterPurchases
AS
SELECT        StartOn, InvoiceNumber, AccountCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, PaymentTerms, Notes, Printed, AccountName, UserName, 
                         InvoiceStatus, CashModeCode, InvoiceType, (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) AS UnpaidValue
FROM            Invoice.vwRegister
WHERE        (InvoiceTypeCode > 1);
go
DROP VIEW Invoice.vwHistoryPurchases
go
CREATE OR ALTER VIEW Invoice.vwHistoryPurchases
AS
SELECT        App.tbYearPeriod.YearNumber, App.tbYear.Description, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, Invoice.vwRegister.StartOn, 
                         Invoice.vwRegister.InvoiceNumber, Invoice.vwRegister.AccountCode, Invoice.vwRegister.InvoiceTypeCode, Invoice.vwRegister.InvoiceStatusCode, Invoice.vwRegister.InvoicedOn, 
                         Invoice.vwRegister.InvoiceValue, Invoice.vwRegister.TaxValue, Invoice.vwRegister.PaidValue, Invoice.vwRegister.PaidTaxValue, Invoice.vwRegister.PaymentTerms, Invoice.vwRegister.Notes, 
                         Invoice.vwRegister.Printed, Invoice.vwRegister.AccountName, Invoice.vwRegister.UserName, Invoice.vwRegister.InvoiceStatus, Invoice.vwRegister.CashModeCode, Invoice.vwRegister.InvoiceType, 
                         (Invoice.vwRegister.InvoiceValue + Invoice.vwRegister.TaxValue) - (Invoice.vwRegister.PaidValue + Invoice.vwRegister.PaidTaxValue) AS UnpaidValue
FROM            App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
                         App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
                         Invoice.vwRegister ON App.tbYearPeriod.StartOn = Invoice.vwRegister.StartOn
WHERE        (Invoice.vwRegister.InvoiceTypeCode > 1);
go
DROP VIEW Invoice.vwHistorySales
go
CREATE OR ALTER VIEW Invoice.vwHistorySales
AS
SELECT        App.tbYearPeriod.YearNumber, CONCAT(App.tbMonth.MonthName, SPACE(1), YEAR(App.tbYearPeriod.StartOn)) AS PeriodName, Invoice.vwRegister.StartOn, 
                         Invoice.vwRegister.InvoiceNumber, Invoice.vwRegister.AccountCode, Invoice.vwRegister.InvoiceTypeCode, Invoice.vwRegister.InvoiceStatusCode, Invoice.vwRegister.InvoicedOn, 
                         Invoice.vwRegister.InvoiceValue, Invoice.vwRegister.TaxValue, Invoice.vwRegister.PaidValue, Invoice.vwRegister.PaidTaxValue, Invoice.vwRegister.PaymentTerms, Invoice.vwRegister.Notes, 
                         Invoice.vwRegister.Printed, Invoice.vwRegister.AccountName, Invoice.vwRegister.UserName, Invoice.vwRegister.InvoiceStatus, Invoice.vwRegister.CashModeCode, Invoice.vwRegister.InvoiceType, 
                         (Invoice.vwRegister.InvoiceValue + Invoice.vwRegister.TaxValue) - (Invoice.vwRegister.PaidValue + Invoice.vwRegister.PaidTaxValue) AS UnpaidValue
FROM            App.tbYearPeriod INNER JOIN
                         App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
                         App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
                         Invoice.vwRegister ON App.tbYearPeriod.StartOn = Invoice.vwRegister.StartOn
WHERE        (Invoice.vwRegister.InvoiceTypeCode < 2);
go
ALTER VIEW Invoice.vwRegisterCashCodes
AS
	WITH cash_codes AS
	(
		SELECT StartOn, CashCode, CashDescription, CashModeCode, CAST(SUM(InvoiceValue) as float) AS TotalInvoiceValue, CAST(SUM(TaxValue) as float) AS TotalTaxValue
		FROM            Invoice.vwRegisterDetail
		GROUP BY StartOn, CashCode, CashDescription, CashModeCode	
	)
	SELECT cash_codes.StartOn, CONCAT(financial_year.[Description], ' ', app_month.MonthName) PeriodName, CashMode,
		CashCode, CashDescription, TotalInvoiceValue, TotalTaxValue, TotalInvoiceValue + TotalTaxValue as TotalValue		
	FROM cash_codes
		JOIN Cash.tbMode cash_mode ON cash_codes.CashModeCode = cash_mode.CashModeCode
		JOIN App.tbYearPeriod year_period ON cash_codes.StartOn = year_period.StartOn
		JOIN App.tbMonth app_month ON year_period.MonthNumber = app_month.MonthNumber
		JOIN App.tbYear financial_year ON year_period.YearNumber = financial_year.YearNumber;
go
CREATE OR ALTER VIEW Invoice.vwRegisterOverdue
AS
	SELECT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.AccountCode, Org.tbOrg.AccountName, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbInvoice.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, 
							 Invoice.tbType.InvoiceType, DATEDIFF(DD, CURRENT_TIMESTAMP, Invoice.tbInvoice.InvoicedOn) AS UnpaidDays, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.DueOn, Invoice.tbInvoice.ExpectedOn, 
							 CASE Invoice.tbType.CashModeCode WHEN 0 THEN Invoice.tbInvoice.InvoiceValue ELSE Invoice.tbInvoice.InvoiceValue * - 1 END AS InvoiceValue, 
							 CASE Invoice.tbType.CashModeCode WHEN 0 THEN Invoice.tbInvoice.TaxValue ELSE Invoice.tbInvoice.TaxValue * - 1 END AS TaxValue, 
							 CASE Invoice.tbType.CashModeCode WHEN 0 THEN (Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue) 
							 ELSE ((Invoice.tbInvoice.InvoiceValue + Invoice.tbInvoice.TaxValue) - (Invoice.tbInvoice.PaidValue + Invoice.tbInvoice.PaidTaxValue)) * - 1 END AS UnpaidValue, 
							 Invoice.tbInvoice.PaymentTerms, Invoice.tbInvoice.Notes
	FROM            Invoice.tbInvoice INNER JOIN
							 Invoice.tbStatus ON Invoice.tbInvoice.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode
	WHERE    (Invoice.tbInvoice.InvoiceStatusCode < 3);
go
CREATE OR ALTER PROCEDURE App.proc_DocDespoolAll
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRAN
		UPDATE Task.tbTask
		SET Spooled = 0, Printed = 1;

		UPDATE  Invoice.tbInvoice
		SET  Spooled = 0, Printed = 1;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
CREATE OR ALTER PROCEDURE Invoice.proc_CancelById(@UserId nvarchar(10))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		BEGIN TRAN

		UPDATE       Task
		SET                TaskStatusCode = 2
		FROM            Task.tbTask AS Task INNER JOIN
								 Invoice.tbTask AS InvoiceTask ON Task.TaskCode = InvoiceTask.TaskCode AND Task.TaskCode = InvoiceTask.TaskCode INNER JOIN
								 Invoice.tbInvoice ON InvoiceTask.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber 
		WHERE        (Invoice.tbInvoice.InvoiceTypeCode = 0 OR Invoice.tbInvoice.InvoiceTypeCode = 2) 
			AND (Invoice.tbInvoice.InvoiceStatusCode = 0) AND (Task.TaskStatusCode = 3) AND (Invoice.tbInvoice.UserId = @UserId)
	                      
		DELETE Invoice.tbInvoice
		FROM         Invoice.tbInvoice INNER JOIN
							  Usr.vwCredentials ON Invoice.tbInvoice.UserId = Usr.vwCredentials.UserId
		WHERE     ( Invoice.tbInvoice.InvoiceStatusCode = 0) AND (Invoice.tbInvoice.UserId = @UserId)
		
		COMMIT TRAN

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
ALTER PROCEDURE Invoice.proc_Cancel
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		DECLARE @UserId nvarchar(10) = (SELECT TOP 1 UserId FROM Usr.vwCredentials)
		EXEC Invoice.proc_CancelById @UserId

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go

ALTER TRIGGER Invoice.Invoice_tbInvoice_TriggerUpdate
ON Invoice.tbInvoice
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		IF UPDATE(Spooled)
		BEGIN
			INSERT INTO App.tbDocSpool (DocTypeCode, DocumentNumber)
			SELECT     App.fnDocInvoiceType(i.InvoiceTypeCode) AS DocTypeCode, i.InvoiceNumber
			FROM         inserted i 
			WHERE     (i.Spooled <> 0)

			DELETE App.tbDocSpool
			FROM         inserted i INNER JOIN
								  App.tbDocSpool ON i.InvoiceNumber = App.tbDocSpool.DocumentNumber
			WHERE    (i.Spooled = 0) AND ( App.tbDocSpool.DocTypeCode > 3)
		END


		IF UPDATE(InvoicedOn) AND EXISTS (
				SELECT * FROM inserted JOIN deleted 
					ON inserted.InvoiceNumber = deleted.InvoiceNumber AND inserted.DueOn = deleted.DueOn)
		BEGIN
			UPDATE invoice
			SET DueOn = App.fnAdjustToCalendar(CASE WHEN org.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, org.PaymentDays, i.InvoicedOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, org.PaymentDays, invoice.InvoicedOn)	
													END, 0)		
				FROM Invoice.tbInvoice invoice
					JOIN inserted i ON i.InvoiceNumber = invoice.InvoiceNumber
					JOIN Org.tbOrg org ON i.AccountCode = org.AccountCode
				WHERE i.InvoiceTypeCode = 0;
		END;	

		IF UPDATE(InvoicedOn) AND EXISTS (
				SELECT * FROM inserted JOIN deleted 
					ON inserted.InvoiceNumber = deleted.InvoiceNumber AND inserted.ExpectedOn = deleted.ExpectedOn)
		BEGIN
			UPDATE invoice
			SET ExpectedOn = App.fnAdjustToCalendar(CASE WHEN org.PayDaysFromMonthEnd <> 0 
													THEN 
														DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, org.PaymentDays + org.ExpectedDays, i.InvoicedOn), 'yyyyMM'), '01')))												
													ELSE
														DATEADD(d, org.PaymentDays + org.ExpectedDays, invoice.InvoicedOn)	
													END, 0)		
				FROM Invoice.tbInvoice invoice
					JOIN inserted i ON i.InvoiceNumber = invoice.InvoiceNumber
					JOIN Org.tbOrg org ON i.AccountCode = org.AccountCode
				WHERE i.InvoiceTypeCode = 0;
		END;	
		
		WITH invoices AS
		(
			SELECT inserted.InvoiceNumber, inserted.AccountCode, inserted.InvoiceStatusCode, inserted.DueOn, inserted.InvoiceValue, inserted.TaxValue, inserted.PaidValue, inserted.PaidTaxValue FROM inserted JOIN deleted ON inserted.InvoiceNumber = deleted.InvoiceNumber WHERE inserted.InvoiceStatusCode = 1 AND deleted.InvoiceStatusCode = 0
		)
		INSERT INTO Invoice.tbChangeLog (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue)
		SELECT InvoiceNumber, orgs.TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue
		FROM invoices JOIN Org.tbOrg orgs ON invoices.AccountCode = orgs.AccountCode;

		IF UPDATE(InvoiceStatusCode) OR UPDATE(DueOn) OR UPDATE(PaidValue) OR UPDATE(PaidTaxValue) OR UPDATE(InvoiceValue) OR UPDATE (TaxValue)
		BEGIN
			WITH candidates AS
			(
				SELECT InvoiceNumber, AccountCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue 
				FROM inserted
				WHERE EXISTS (SELECT * FROM Invoice.tbChangeLog WHERE InvoiceNumber = inserted.InvoiceNumber)
			)
			, logs AS
			(
				SELECT clog.LogId, clog.InvoiceNumber, clog.InvoiceStatusCode, clog.TransmitStatusCode, clog.DueOn, clog.InvoiceValue, clog.TaxValue, clog.PaidValue, clog.PaidTaxValue 
				FROM Invoice.tbChangeLog clog
				JOIN candidates ON clog.InvoiceNumber = candidates.InvoiceNumber AND LogId = (SELECT MAX(LogId) FROM Invoice.tbChangeLog WHERE InvoiceNumber = candidates.InvoiceNumber)		
			)
			INSERT INTO Invoice.tbChangeLog
									 (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue)
			SELECT candidates.InvoiceNumber, CASE orgs.TransmitStatusCode WHEN 1 THEN 2 ELSE 0 END TransmitStatusCode, candidates.InvoiceStatusCode,
				candidates.DueOn, candidates.InvoiceValue, candidates.TaxValue, candidates.PaidValue, candidates.PaidTaxValue
			FROM candidates 
				JOIN Org.tbOrg orgs ON candidates.AccountCode = orgs.AccountCode 
				JOIN logs ON candidates.InvoiceNumber = logs.InvoiceNumber
			WHERE (logs.InvoiceStatusCode <> candidates.InvoiceStatusCode) 
				OR (logs.TransmitStatusCode < 2)
				OR (logs.DueOn <> candidates.DueOn) 
				OR ((logs.InvoiceValue + logs.TaxValue + logs.PaidValue + logs.PaidTaxValue) 
						<> (candidates.InvoiceValue + candidates.TaxValue + candidates.PaidValue + candidates.PaidTaxValue))
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
CREATE OR ALTER PROCEDURE Cash.proc_TaxAdjustment (@StartOn datetime, @TaxTypeCode smallint, @TaxAdjustment decimal(18, 5))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		DECLARE 		
			@PayTo datetime,
			@PayFrom datetime;

		SELECT 
			@PayFrom = PayFrom,
			@PayTo = PayTo 
		FROM Cash.fnTaxTypeDueDates(@TaxTypeCode) due_dates 
		WHERE @StartOn >= due_dates.PayFrom AND @StartOn < due_dates.PayTo

		UPDATE App.tbYearPeriod
		SET 
			TaxAdjustment = CASE @TaxTypeCode WHEN 0 THEN 0 ELSE TaxAdjustment END,
			VatAdjustment = CASE @TaxTypeCode WHEN 1 THEN 0 ELSE VatAdjustment END
		WHERE StartOn >= @PayFrom AND StartOn < @PayTo;

		SELECT @StartOn = MAX(StartOn)
		FROM App.tbYearPeriod
		WHERE StartOn < @PayTo;

		UPDATE App.tbYearPeriod
		SET 
			TaxAdjustment = CASE @TaxTypeCode WHEN 0 THEN @TaxAdjustment ELSE TaxAdjustment END,
			VatAdjustment = CASE @TaxTypeCode WHEN 1 THEN @TaxAdjustment ELSE VatAdjustment END
		WHERE StartOn = @StartOn;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
ALTER VIEW Cash.vwTaxCorpTotals
AS
	WITH totals AS
	(
		SELECT App.tbYearPeriod.YearNumber, netprofit_totals.StartOn, YEAR(App.tbYearPeriod.StartOn) AS PeriodYear, App.tbYear.Description, 
						  App.tbMonth.MonthName + ' ' + LTRIM(STR(YEAR(App.tbYearPeriod.StartOn))) AS Period, App.tbYearPeriod.CorporationTaxRate, 
						  App.tbYearPeriod.TaxAdjustment, SUM(netprofit_totals.NetProfit) AS NetProfit, SUM(netprofit_totals.CorporationTax) AS CorporationTax
		FROM       Cash.vwTaxCorpTotalsByPeriod  netprofit_totals INNER JOIN
							  App.tbYearPeriod ON netprofit_totals.StartOn = App.tbYearPeriod.StartOn INNER JOIN
							  App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber INNER JOIN
							  App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber
		WHERE     (App.tbYear.CashStatusCode BETWEEN 1 AND 2)
		GROUP BY App.tbYearPeriod.YearNumber, App.tbYear.Description, App.tbMonth.MonthName, netprofit_totals.StartOn, YEAR(App.tbYearPeriod.StartOn), 
							  App.tbYearPeriod.CorporationTaxRate, App.tbYearPeriod.TaxAdjustment
	)
	SELECT YearNumber, StartOn, PeriodYear, Description, Period, CorporationTaxRate, TaxAdjustment, CAST(NetProfit AS decimal(18, 5)) NetProfit, CAST(CorporationTax AS decimal(18, 5)) CorporationTax
	FROM totals;

go
CREATE OR ALTER PROCEDURE App.proc_TaxRates(@StartOn datetime, @EndOn datetime, @CorporationTaxRate real)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY	
		UPDATE App.tbYearPeriod
		SET CorporationTaxRate = @CorporationTaxRate
		WHERE StartOn >= @StartOn AND StartOn <= @EndOn;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
go
ALTER VIEW App.vwTaxCodes
AS
	SELECT        App.tbTaxCode.TaxCode, App.tbTaxCode.TaxDescription, Cash.tbTaxType.TaxType, App.tbTaxCode.TaxTypeCode, App.tbTaxCode.RoundingCode, App.tbRounding.Rounding, App.tbTaxCode.TaxRate, App.tbTaxCode.Decimals, 
							 App.tbTaxCode.UpdatedBy, App.tbTaxCode.UpdatedOn
	FROM            App.tbTaxCode INNER JOIN
							 Cash.tbTaxType ON App.tbTaxCode.TaxTypeCode = Cash.tbTaxType.TaxTypeCode INNER JOIN
							 App.tbRounding ON App.tbTaxCode.RoundingCode = App.tbRounding.RoundingCode

go

ALTER TRIGGER App.App_tbTaxCode_TriggerUpdate ON App.tbTaxCode AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(TaxCode) = 0)
		BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1);
			ROLLBACK TRANSACTION;
		END
		ELSE IF NOT UPDATE(UpdatedBy)
		BEGIN
			UPDATE App.tbTaxCode
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM App.tbTaxCode INNER JOIN inserted AS i ON tbTaxCode.TaxCode = i.TaxCode;
		END
		
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
ALTER VIEW Cash.vwBalanceSheet
AS
	WITH balance_sheets AS
	(

		SELECT AssetCode, AssetName, CashModeCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetOrgs
		UNION
		SELECT AssetCode, AssetName, CashModeCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetAccounts
		UNION 
		SELECT AssetCode, AssetName, CashModeCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetAssets
		UNION 
		SELECT AssetCode, AssetName, CashModeCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetTax
		UNION
		SELECT AssetCode, AssetName, CashModeCode, AssetTypeCode, StartOn, Balance FROM Cash.vwBalanceSheetVat

	), balance_sheet_unordered AS
	(
		SELECT 
			balance_sheet_periods.AssetCode, balance_sheet_periods.AssetName,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN balance_sheet_periods.CashModeCode 
				ELSE balance_sheets.CashModeCode 
			END CashModeCode, LiquidityLevel,
			balance_sheet_periods.StartOn,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN 0 
				ELSE balance_sheets.Balance 
			END Balance,
			CASE WHEN balance_sheets.AssetCode IS NULL 
				THEN balance_sheet_periods.IsEntry 
				ELSE CAST(1 as bit) 
			END IsEntry
		FROM Cash.vwBalanceSheetPeriods balance_sheet_periods
			LEFT OUTER JOIN balance_sheets
				ON balance_sheet_periods.AssetCode = balance_sheets.AssetCode
					AND balance_sheet_periods.AssetName = balance_sheets.AssetName
					AND balance_sheet_periods.CashModeCode = balance_sheets.CashModeCode
					AND balance_sheet_periods.StartOn = balance_sheets.StartOn
	), balance_sheet_ordered AS
	(
		SELECT 
			ROW_NUMBER() OVER (ORDER BY CashModeCode desc, LiquidityLevel desc, AssetName, StartOn) EntryNumber,
			AssetCode, AssetName, CashModeCode, LiquidityLevel, StartOn, Balance, IsEntry
		FROM balance_sheet_unordered
	), balance_sheet_ranked AS
	(
		SELECT *, 
		RANK() OVER (PARTITION BY AssetName, CashModeCode, IsEntry ORDER BY EntryNumber) RNK
		FROM balance_sheet_ordered
	), balance_sheet_grouped AS
	(
		SELECT EntryNumber, AssetCode, AssetName, CashModeCode, LiquidityLevel, StartOn, Balance, IsEntry,
		MAX(CASE IsEntry WHEN 0 THEN 0 ELSE RNK END) OVER (PARTITION BY AssetName, CashModeCode ORDER BY EntryNumber) RNK
		FROM balance_sheet_ranked
	)
	SELECT EntryNumber, AssetCode, AssetName, CashModeCode, LiquidityLevel, balance_sheet_grouped.StartOn, 
		year_period.YearNumber, year_period.MonthNumber, IsEntry,
		CASE IsEntry WHEN 0 THEN
			MAX(Balance) OVER (PARTITION BY AssetName, CashModeCode, RNK ORDER BY EntryNumber) +
			MIN(Balance) OVER (PARTITION BY AssetName, CashModeCode, RNK ORDER BY EntryNumber) 
		ELSE
			Balance
		END AS Balance
	FROM balance_sheet_grouped
		JOIN App.tbYearPeriod year_period ON balance_sheet_grouped.StartOn = year_period.StartOn;

go
ALTER VIEW App.vwPeriods
AS
	SELECT TOP (100) PERCENT App.tbYear.YearNumber, App.tbYearPeriod.MonthNumber, App.tbYearPeriod.StartOn, App.tbYear.Description + SPACE(1) + App.tbMonth.MonthName AS Description, App.tbYearPeriod.RowVer
	FROM            App.tbYearPeriod INNER JOIN
							 App.tbMonth ON App.tbYearPeriod.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							 App.tbYear ON App.tbYearPeriod.YearNumber = App.tbYear.YearNumber
	WHERE        (App.tbYear.CashStatusCode < 3) AND (App.tbYearPeriod.CashStatusCode < 3)
go
CREATE OR ALTER VIEW Cash.vwProfitAndLossData
AS
	WITH category_relations AS
	(
		SELECT Cash.tbCategoryTotal.ParentCode, Cash.tbCategoryTotal.ChildCode, 
			Cash.tbCategory.CategoryTypeCode, Cash.tbCode.CashCode, Cash.tbCategory.CashTypeCode, Cash.tbCategory.CashModeCode
		FROM  Cash.tbCategoryTotal 
			INNER JOIN Cash.tbCategory ON Cash.tbCategoryTotal.ChildCode = Cash.tbCategory.CategoryCode 
			LEFT OUTER JOIN Cash.tbCode ON Cash.tbCategory.CategoryCode = Cash.tbCode.CategoryCode
	), categories AS
	(
		SELECT CategoryCode
		FROM  Cash.tbCategory category 
		WHERE ( CashTypeCode = 0) AND (CategoryTypeCode = 1)
			AND NOT EXISTS (SELECT * FROM App.tbOptions o WHERE o.VatCategoryCode = category.CategoryCode) 
			
	), cashcode_candidates AS
	(
		SELECT   categories.CategoryCode, ChildCode, CashCode, CashTypeCode, CashModeCode
		FROM category_relations
			JOIN categories ON category_relations.ParentCode = categories.CategoryCode		

		UNION ALL

		SELECT  cashcode_candidates.CategoryCode, category_relations.ChildCode, category_relations.CashCode, category_relations.CashTypeCode, category_relations.CashModeCode
		FROM  category_relations JOIN cashcode_candidates ON category_relations.ParentCode = cashcode_candidates.ChildCode
	), cashcode_selected AS
	(
		SELECT CategoryCode, CashCode, CashTypeCode, CashModeCode FROM cashcode_candidates
		UNION
		SELECT ParentCode CategoryCode, CashCode, CashTypeCode, CashModeCode FROM category_relations WHERE ParentCode = (SELECT NetProfitCode FROM App.tbOptions)
	), category_cash_codes AS
	(
		SELECT DISTINCT CategoryCode, CashCode, CashTypeCode, CashModeCode
		FROM cashcode_selected WHERE NOT CashCode IS NULL
	), active_periods AS
	(
		SELECT yr.YearNumber, pd.StartOn
		FROM App.tbYear yr
			JOIN App.tbYearPeriod pd ON yr.YearNumber = pd.YearNumber
		WHERE (yr.CashStatusCode BETWEEN 1 AND 2)
	), category_data AS
	(
		SELECT category_cash_codes.CategoryCode, periods.CashCode, periods.StartOn, 
			CASE category_cash_codes.CashModeCode WHEN 0 THEN periods.InvoiceValue * -1 ELSE InvoiceValue END InvoiceValue
		FROM category_cash_codes 
			JOIN Cash.tbPeriod periods ON category_cash_codes.CashCode = periods.CashCode
			JOIN active_periods ON active_periods.StartOn = periods.StartOn
	)
	SELECT CategoryCode, StartOn, SUM(InvoiceValue) InvoiceValue
	FROM category_data
	GROUP BY CategoryCode, StartOn;
go
CREATE OR ALTER VIEW Cash.vwProfitAndLossByMonth
AS
	SELECT category.CategoryCode, category.Category, periods.YearNumber, periods.MonthNumber, category.DisplayOrder, financial_year.Description,
		year_month.MonthName, profit_data.StartOn, profit_data.InvoiceValue
	FROM Cash.vwProfitAndLossData profit_data
		JOIN Cash.tbCategory category ON profit_data.CategoryCode = category.CategoryCode
		JOIN App.tbYearPeriod periods ON profit_data.StartOn = periods.StartOn
		JOIN App.tbYear financial_year ON periods.YearNumber = financial_year.YearNumber
		JOIN App.tbMonth year_month ON periods.MonthNumber = year_month.MonthNumber;
go
CREATE OR ALTER VIEW Cash.vwProfitAndLossByYear
AS
	SELECT financial_year.YearNumber, financial_year.Description, category.DisplayOrder, category.CategoryCode, category.Category, SUM(profit_data.InvoiceValue) InvoiceValue
	FROM Cash.vwProfitAndLossData profit_data
		JOIN Cash.tbCategory category ON profit_data.CategoryCode = category.CategoryCode
		JOIN App.tbYearPeriod periods ON profit_data.StartOn = periods.StartOn
		JOIN App.tbYear financial_year ON periods.YearNumber = financial_year.YearNumber
	GROUP BY financial_year.YearNumber, financial_year.Description, category.DisplayOrder, category.CategoryCode, category.Category;
go
ALTER VIEW Org.vwCashAccounts
AS
SELECT        Org.tbAccount.CashAccountCode, Org.tbOrg.AccountCode, Org.tbAccount.CashAccountName, Org.tbOrg.AccountName, Org.tbType.OrganisationType, Org.tbAccount.OpeningBalance, Org.tbAccount.CurrentBalance, 
                         Org.tbAccount.SortCode, Org.tbAccount.AccountNumber, Org.tbAccount.AccountClosed, Org.tbAccount.AccountTypeCode, Org.tbAccountType.AccountType, Org.tbAccount.CashCode, Cash.tbCode.CashDescription, 
                         Org.tbAccount.InsertedBy, Org.tbAccount.InsertedOn, Org.tbAccount.LiquidityLevel
FROM            Org.tbOrg INNER JOIN
                         Org.tbAccount ON Org.tbOrg.AccountCode = Org.tbAccount.AccountCode INNER JOIN
                         Org.tbType ON Org.tbOrg.OrganisationTypeCode = Org.tbType.OrganisationTypeCode INNER JOIN
                         Org.tbAccountType ON Org.tbAccount.AccountTypeCode = Org.tbAccountType.AccountTypeCode LEFT OUTER JOIN
                         Cash.tbCode ON Org.tbAccount.CashCode = Cash.tbCode.CashCode AND Org.tbAccount.CashCode = Cash.tbCode.CashCode AND Org.tbAccount.CashCode = Cash.tbCode.CashCode
go
ALTER TRIGGER Cash.Cash_tbCategory_TriggerUpdate 
   ON  Cash.tbCategory
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(CategoryCode) = 0)
		BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
		END

		IF UPDATE (IsEnabled)
		BEGIN
			UPDATE  Cash.tbCode
			SET     IsEnabled = 0
			FROM        inserted INNER JOIN
										Cash.tbCode ON inserted.CategoryCode = Cash.tbCode.CategoryCode
			WHERE        (inserted.IsEnabled = 0) AND (Cash.tbCode.IsEnabled <> 0);
		END

		IF NOT UPDATE(UpdatedBy)
		BEGIN
			UPDATE Cash.tbCategory
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Cash.tbCategory INNER JOIN inserted AS i ON tbCategory.CategoryCode = i.CategoryCode;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
CREATE OR ALTER VIEW Cash.vwCode
AS
	SELECT        Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCode.CategoryCode, Cash.tbCategory.Category, Cash.tbMode.CashModeCode, Cash.tbMode.CashMode, App.tbTaxCode.TaxDescription, 
							 Cash.tbCategory.CashTypeCode, Cash.tbType.CashType, CAST(Cash.tbCode.IsEnabled AS bit) AS IsCashEnabled, CAST(Cash.tbCategory.IsEnabled AS bit) AS IsCategoryEnabled, Cash.tbCode.InsertedBy, 
							 Cash.tbCode.InsertedOn, Cash.tbCode.UpdatedBy, Cash.tbCode.UpdatedOn
	FROM            Cash.tbCode INNER JOIN
							 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode INNER JOIN
							 Cash.tbMode ON Cash.tbCategory.CashModeCode = Cash.tbMode.CashModeCode INNER JOIN
							 Cash.tbType ON Cash.tbCategory.CashTypeCode = Cash.tbType.CashTypeCode INNER JOIN
							 App.tbTaxCode ON Cash.tbCode.TaxCode = App.tbTaxCode.TaxCode
go
ALTER TRIGGER Cash.Cash_tbCode_TriggerUpdate
   ON  Cash.tbCode
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(CashCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1);
			ROLLBACK
			END
		ELSE IF NOT UPDATE(UpdatedBy)
			BEGIN
			UPDATE Cash.tbCode
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Cash.tbCode INNER JOIN inserted AS i ON tbCode.CashCode = i.CashCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
go
ALTER VIEW Cash.vwAccountStatement
AS
	WITH entries AS
	(
		SELECT  payment.CashAccountCode, payment.CashCode, ROW_NUMBER() OVER (PARTITION BY payment.CashAccountCode ORDER BY PaidOn) AS EntryNumber, PaymentCode, PaidOn, 
			CASE WHEN PaidInValue > 0 THEN PaidInValue ELSE PaidOutValue * - 1 END AS Paid
		FROM         Cash.tbPayment payment INNER JOIN Org.tbAccount ON payment.CashAccountCode = Org.tbAccount.CashAccountCode
		WHERE     (PaymentStatusCode = 1) AND (AccountClosed = 0)	
		UNION
		SELECT        
			CashAccountCode, 
			COALESCE(CashCode, (SELECT TOP 1 CashCode FROM Cash.vwBankCashCodes WHERE CashModeCode = 2)) CashCode,
			0 AS EntryNumber, 
			(SELECT CAST(Message AS NVARCHAR(30)) FROM App.tbText WHERE TextId = 3005) AS PaymentCode, 
			DATEADD(HOUR, - 1, (SELECT MIN(PaidOn) FROM Cash.tbPayment WHERE CashAccountCode = cash_account.CashAccountCode)) AS PaidOn, OpeningBalance AS Paid
		FROM            Org.tbAccount cash_account 								 
		WHERE        (AccountClosed = 0) 
	), running_balance AS
	(
		SELECT CashAccountCode, CashCode, EntryNumber, PaymentCode, PaidOn, 
			SUM(Paid) OVER (PARTITION BY CashAccountCode ORDER BY EntryNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS PaidBalance
		FROM entries
	), payments AS
	(
		SELECT     Cash.tbPayment.PaymentCode, Cash.tbPayment.CashAccountCode, Usr.tbUser.UserName, Cash.tbPayment.AccountCode, 
							  Org.tbOrg.AccountName, Cash.tbPayment.CashCode, Cash.tbCode.CashDescription, App.tbTaxCode.TaxDescription, 
							  Cash.tbPayment.PaidInValue, Cash.tbPayment.PaidOutValue, 
							  Cash.tbPayment.PaymentReference, Cash.tbPayment.InsertedBy, Cash.tbPayment.InsertedOn, 
							  Cash.tbPayment.UpdatedBy, Cash.tbPayment.UpdatedOn, Cash.tbPayment.TaxCode
		FROM         Cash.tbPayment INNER JOIN
							  Usr.tbUser ON Cash.tbPayment.UserId = Usr.tbUser.UserId INNER JOIN
							  Org.tbOrg ON Cash.tbPayment.AccountCode = Org.tbOrg.AccountCode LEFT OUTER JOIN
							  App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							  Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode
	)
		SELECT running_balance.CashAccountCode, 
			COALESCE((SELECT TOP 1 StartOn FROM App.tbYearPeriod WHERE (StartOn <= running_balance.PaidOn) ORDER BY StartOn DESC), 
				(SELECT MIN(StartOn) FROM App.tbYearPeriod) ) AS StartOn, 
			running_balance.EntryNumber, running_balance.PaymentCode, running_balance.PaidOn, 
			payments.AccountName, payments.PaymentReference, COALESCE(payments.PaidInValue, 0) PaidInValue, 
			COALESCE(payments.PaidOutValue, 0) PaidOutValue, CAST(running_balance.PaidBalance as decimal(18,5)) PaidBalance, 
			payments.CashCode, payments.CashDescription, payments.TaxDescription, payments.UserName, 
			payments.AccountCode, payments.TaxCode
		FROM   running_balance LEFT OUTER JOIN
								payments ON running_balance.PaymentCode = payments.PaymentCode

go
CREATE OR ALTER VIEW App.vwTaxTypes
AS
	SELECT        Cash.tbTaxType.TaxTypeCode, Cash.tbTaxType.TaxType, Cash.tbTaxType.CashCode, Cash.tbCode.CashDescription, Cash.tbTaxType.MonthNumber, App.tbMonth.MonthName, Cash.tbTaxType.RecurrenceCode, 
							 App.tbRecurrence.Recurrence, Cash.tbTaxType.AccountCode, Org.tbOrg.AccountName, Cash.tbTaxType.OffsetDays
	FROM            Cash.tbTaxType INNER JOIN
							 Cash.tbCode ON Cash.tbTaxType.CashCode = Cash.tbCode.CashCode INNER JOIN
							 App.tbMonth ON Cash.tbTaxType.MonthNumber = App.tbMonth.MonthNumber INNER JOIN
							 App.tbRecurrence ON Cash.tbTaxType.RecurrenceCode = App.tbRecurrence.RecurrenceCode INNER JOIN
							 Org.tbOrg ON Cash.tbTaxType.AccountCode = Org.tbOrg.AccountCode;
go

