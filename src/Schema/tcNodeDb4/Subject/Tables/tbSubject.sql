CREATE TABLE [Subject].[tbSubject] (
    [SubjectCode]            NVARCHAR (50)   NOT NULL,
    [SubjectName]            NVARCHAR (255)  NOT NULL,
    [SubjectTypeCode]        SMALLINT        CONSTRAINT [DF_Subject_tb_SubjectTypeCode] DEFAULT ((1)) NOT NULL,
    [SubjectStatusCode]      SMALLINT        CONSTRAINT [DF_Subject_tb_SubjectStatusCode] DEFAULT ((1)) NOT NULL,
    [TransmitStatusCode]     SMALLINT        CONSTRAINT [DF_Subject_tbSubject_TransmitStatusCode] DEFAULT ((0)) NOT NULL,
    [TaxCode]                NVARCHAR (10)   NULL,
    [AddressCode]            NVARCHAR (15)   NULL,
    [PaymentTerms]           NVARCHAR (100) NULL,
    [ExpectedDays]           SMALLINT       CONSTRAINT [DF_Subject_ExpectedDays] DEFAULT ((0)) NOT NULL,
    [PaymentDays]            SMALLINT       CONSTRAINT [DF_Subject_PaymentDays] DEFAULT ((0)) NOT NULL,
    [PayDaysFromMonthEnd]    BIT           CONSTRAINT [DF_Subject_PayDaysFromMonthEnd] DEFAULT ((0)) NOT NULL,
    [PayBalance]             BIT            CONSTRAINT [DF_Subject_PayBalance] DEFAULT ((1)) NOT NULL,
    [OpeningBalance]         DECIMAL (18, 5) CONSTRAINT [DF_Subject_OpeningBalance] DEFAULT ((0)) NOT NULL,
    [AreaCode]               NVARCHAR (50)   NULL,
    [PhoneNumber]            NVARCHAR (50)   NULL,
    [EmailAddress]           NVARCHAR (255)  NULL,
    [InsertedBy]             NVARCHAR (50)   CONSTRAINT [DF_Subject_tb_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]             DATETIME        CONSTRAINT [DF_Subject_tb_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]              NVARCHAR (50)   CONSTRAINT [DF_Subject_tb_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]              DATETIME        CONSTRAINT [DF_Subject_tb_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]                 ROWVERSION      NOT NULL,
    -- legacy fields moved to tbVirtual
    -- end of legacy fields
    CONSTRAINT [PK_Subject_tbSubject] PRIMARY KEY NONCLUSTERED ([SubjectCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Subject_tb_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Subject_tb_Subject_tbAddress] FOREIGN KEY ([AddressCode]) REFERENCES [Subject].[tbAddress] ([AddressCode]) NOT FOR REPLICATION,
    CONSTRAINT [FK_Subject_tbSubject_tbTransmitStatus] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode]),
    CONSTRAINT [FK_Subject_tbSubject_tbStatus] FOREIGN KEY ([SubjectStatusCode]) REFERENCES [Subject].[tbStatus] ([SubjectStatusCode]),
    CONSTRAINT [FK_Subject_tbSubject_tbType] FOREIGN KEY ([SubjectTypeCode]) REFERENCES [Subject].[tbType] ([SubjectTypeCode])
);


GO
ALTER TABLE [Subject].[tbSubject] NOCHECK CONSTRAINT [FK_Subject_tb_Subject_tbAddress];


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tb_AccountName]
    ON [Subject].[tbSubject]([SubjectName] ASC) WITH (FILLFACTOR = 90);

GO
CREATE NONCLUSTERED INDEX [IX_Subject_tb_AreaCode]
    ON [Subject].[tbSubject]([AreaCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tb_SubjectStatusCode]
    ON [Subject].[tbSubject]([SubjectStatusCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tb_Status_AccountCode]
    ON [Subject].[tbSubject]([SubjectStatusCode] ASC, [SubjectName] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Subject_tb_SubjectTypeCode]
    ON [Subject].[tbSubject]([SubjectTypeCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_tbSubject_tb_AccountCode]
    ON [Subject].[tbSubject]([SubjectCode] ASC)
    INCLUDE([SubjectName]);


GO
CREATE TRIGGER Subject.Subject_tbSubject_TriggerUpdate 
   ON  Subject.tbSubject
   AFTER UPDATE, INSERT
AS 
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(SubjectCode) = 0)
        BEGIN
            DECLARE @Msg NVARCHAR(MAX);
            SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
            RAISERROR (@Msg, 10, 1);
            ROLLBACK TRANSACTION;
        END
        ELSE
        BEGIN
            UPDATE Subject.tbSubject
            SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
            FROM Subject.tbSubject INNER JOIN inserted AS i ON tbSubject.SubjectCode = i.SubjectCode;
        END
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
END
GO
GO
CREATE TRIGGER [Subject].[Subject_tbSubject_TriggerDelete]
ON [Subject].[tbSubject]
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DELETE n
        FROM Subject.tbNamespace AS n
        WHERE EXISTS
        (
            SELECT 1
            FROM deleted AS d
            WHERE d.SubjectCode = n.ParentSubjectCode
               OR d.SubjectCode = n.ChildSubjectCode
        );
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
END
