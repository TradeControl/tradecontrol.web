CREATE TABLE [Subject].[tbContact] (
    [SubjectCode]   NVARCHAR (10)  NOT NULL,
    [ContactName]   NVARCHAR (100) NOT NULL,
    [FileAs]        NVARCHAR (100) NULL,
    [OnMailingList] BIT            CONSTRAINT [DF_Subject_tbContact_OnMailingList] DEFAULT ((1)) NOT NULL,
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
    [Information]   NVARCHAR(MAX)          NULL,
    [Photo]         VARBINARY(MAX)          NULL,
    [InsertedBy]    NVARCHAR (50)  CONSTRAINT [DF_Subject_tbContact_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]    DATETIME       CONSTRAINT [DF_Subject_tbContact_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]     NVARCHAR (50)  CONSTRAINT [DF_Subject_tbContact_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]     DATETIME       CONSTRAINT [DF_Subject_tbContact_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]        ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Subject_tbContact] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC, [ContactName] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbContact_AccountCode] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbContactDepartment]
    ON [Subject].[tbContact]([Department] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbContactJobTitle]
    ON [Subject].[tbContact]([JobTitle] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbContactNameTitle]
    ON [Subject].[tbContact]([NameTitle] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tbContact_AccountCode]
    ON [Subject].[tbContact]([SubjectCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Subject.Subject_tbContact_TriggerInsert 
   ON  Subject.tbContact
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
	
		UPDATE Subject.tbContact
		SET 
			NickName = RTRIM(CASE 
				WHEN LEN(ISNULL(i.NickName, '')) > 0 THEN i.NickName
				WHEN CHARINDEX(' ', tbContact.ContactName, 0) = 0 THEN tbContact.ContactName 
				ELSE LEFT(tbContact.ContactName, CHARINDEX(' ', tbContact.ContactName, 0)) END),
			FileAs = Subject.fnContactFileAs(tbContact.ContactName)
		FROM Subject.tbContact INNER JOIN inserted AS i ON tbContact.SubjectCode = i.SubjectCode AND tbContact.ContactName = i.ContactName;

	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		EXEC App.proc_ErrorLog;
		THROW;
	END CATCH
END

GO
CREATE   TRIGGER Subject.Subject_tbContact_TriggerUpdate 
   ON  Subject.tbContact
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	

		IF UPDATE(ContactName)
		BEGIN
			UPDATE Subject.tbContact
			SET 
				FileAs = Subject.fnContactFileAs(tbContact.ContactName)
			FROM Subject.tbContact INNER JOIN inserted AS i ON tbContact.SubjectCode = i.SubjectCode AND tbContact.ContactName = i.ContactName;
		END

		UPDATE Subject.tbContact
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Subject.tbContact INNER JOIN inserted AS i ON tbContact.SubjectCode = i.SubjectCode AND tbContact.ContactName = i.ContactName;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END

