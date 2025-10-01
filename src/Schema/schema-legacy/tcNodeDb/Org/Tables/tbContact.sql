CREATE TABLE [Org].[tbContact] (
    [AccountCode]   NVARCHAR (10)  NOT NULL,
    [ContactName]   NVARCHAR (100) NOT NULL,
    [FileAs]        NVARCHAR (100) NULL,
    [OnMailingList] BIT            CONSTRAINT [DF_Org_tbContact_OnMailingList] DEFAULT ((1)) NOT NULL,
    [NameTitle]     NVARCHAR (25)  NULL,
    [NickName]      NVARCHAR (100) NULL,
    [JobTitle]      NVARCHAR (100) NULL,
    [PhoneNumber]   NVARCHAR (50)  NULL,
    [MobileNumber]  NVARCHAR (50)  NULL,
    [EmailAddress]  NVARCHAR (255) NULL,
    [Hobby]         NVARCHAR (50)  NULL,
    [DateOfBirth]   DATETIME       NULL,
    [Department]    NVARCHAR (50)  NULL,
    [SpouseName]    NVARCHAR (50)  NULL,
    [HomeNumber]    NVARCHAR (50)  NULL,
    [Information]   NTEXT          NULL,
    [Photo]         IMAGE          NULL,
    [InsertedBy]    NVARCHAR (50)  CONSTRAINT [DF_Org_tbContact_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]    DATETIME       CONSTRAINT [DF_Org_tbContact_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]     NVARCHAR (50)  CONSTRAINT [DF_Org_tbContact_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]     DATETIME       CONSTRAINT [DF_Org_tbContact_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]        ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Org_tbContact] PRIMARY KEY NONCLUSTERED ([AccountCode] ASC, [ContactName] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Org_tbContact_AccountCode] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tbContactDepartment]
    ON [Org].[tbContact]([Department] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tbContactJobTitle]
    ON [Org].[tbContact]([JobTitle] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tbContactNameTitle]
    ON [Org].[tbContact]([NameTitle] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tbContact_AccountCode]
    ON [Org].[tbContact]([AccountCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Org.Org_tbContact_TriggerInsert 
   ON  Org.tbContact
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
	
		UPDATE Org.tbContact
		SET 
			NickName = RTRIM(CASE 
				WHEN LEN(ISNULL(i.NickName, '')) > 0 THEN i.NickName
				WHEN CHARINDEX(' ', tbContact.ContactName, 0) = 0 THEN tbContact.ContactName 
				ELSE LEFT(tbContact.ContactName, CHARINDEX(' ', tbContact.ContactName, 0)) END),
			FileAs = Org.fnContactFileAs(tbContact.ContactName)
		FROM Org.tbContact INNER JOIN inserted AS i ON tbContact.AccountCode = i.AccountCode AND tbContact.ContactName = i.ContactName;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		EXEC App.proc_ErrorLog;
		THROW;
	END CATCH
END

GO
CREATE   TRIGGER Org.Org_tbContact_TriggerUpdate 
   ON  Org.tbContact
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	

		IF UPDATE(ContactName)
		BEGIN
			UPDATE Org.tbContact
			SET 
				FileAs = Org.fnContactFileAs(tbContact.ContactName)
			FROM Org.tbContact INNER JOIN inserted AS i ON tbContact.AccountCode = i.AccountCode AND tbContact.ContactName = i.ContactName;
		END

		UPDATE Org.tbContact
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Org.tbContact INNER JOIN inserted AS i ON tbContact.AccountCode = i.AccountCode AND tbContact.ContactName = i.ContactName;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END

