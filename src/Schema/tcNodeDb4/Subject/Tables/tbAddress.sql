CREATE TABLE [Subject].[tbAddress] (
    [AddressCode] NVARCHAR (15) NOT NULL,
    [SubjectCode] NVARCHAR (10) NOT NULL,
    [Address]     NVARCHAR(MAX)         NOT NULL,
    [InsertedBy]  NVARCHAR (50) CONSTRAINT [DF_Subject_tbAddress_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]  DATETIME      CONSTRAINT [DF_Subject_tbAddress_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]   NVARCHAR (50) CONSTRAINT [DF_Subject_tbAddress_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]   DATETIME      CONSTRAINT [DF_Subject_tbAddress_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]      ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Subject_tbAddress] PRIMARY KEY CLUSTERED ([AddressCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tbAddress_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subject_tbAddress]
    ON [Subject].[tbAddress]([SubjectCode] ASC, [AddressCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Subject.Subject_tbAddress_TriggerInsert
ON Subject.tbAddress 
FOR INSERT
AS
	SET NOCOUNT ON;
	BEGIN TRY
		If EXISTS(SELECT     Subject.tbSubject.AddressCode, Subject.tbSubject.SubjectCode
				  FROM         Subject.tbSubject INNER JOIN
										inserted AS i ON Subject.tbSubject.SubjectCode = i.SubjectCode
				  WHERE     ( Subject.tbSubject.AddressCode IS NULL))
			BEGIN
			UPDATE Subject.tbSubject
			SET AddressCode = i.AddressCode
			FROM         Subject.tbSubject INNER JOIN
										inserted AS i ON Subject.tbSubject.SubjectCode = i.SubjectCode
				  WHERE     ( Subject.tbSubject.AddressCode IS NULL)
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH


GO
CREATE   TRIGGER Subject.Subject_tbAddress_TriggerUpdate 
   ON  Subject.tbAddress
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Subject.tbAddress
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Subject.tbAddress INNER JOIN inserted AS i ON tbAddress.AddressCode = i.AddressCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
