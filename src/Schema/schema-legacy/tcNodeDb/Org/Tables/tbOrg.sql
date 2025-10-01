CREATE TABLE [Org].[tbOrg] (
    [AccountCode]            NVARCHAR (10)   NOT NULL,
    [AccountName]            NVARCHAR (255)  NOT NULL,
    [OrganisationTypeCode]   SMALLINT        CONSTRAINT [DF_Org_tb_OrganisationTypeCode] DEFAULT ((1)) NOT NULL,
    [OrganisationStatusCode] SMALLINT        CONSTRAINT [DF_Org_tb_OrganisationStatusCode] DEFAULT ((1)) NOT NULL,
    [TaxCode]                NVARCHAR (10)   NULL,
    [AddressCode]            NVARCHAR (15)   NULL,
    [AreaCode]               NVARCHAR (50)   NULL,
    [PhoneNumber]            NVARCHAR (50)   NULL,
    [EmailAddress]           NVARCHAR (255)  NULL,
    [WebSite]                NVARCHAR (255)  NULL,
    [AccountSource]          NVARCHAR (100)  NULL,
    [PaymentTerms]           NVARCHAR (100)  NULL,
    [ExpectedDays]           SMALLINT        CONSTRAINT [DF_Org_tbOrg_ExpectedDays] DEFAULT ((0)) NOT NULL,
    [PaymentDays]            SMALLINT        CONSTRAINT [DF_Org_tb_PaymentDays] DEFAULT ((0)) NOT NULL,
    [PayDaysFromMonthEnd]    BIT             CONSTRAINT [DF_Org_tb_PayDaysFromMonthEnd] DEFAULT ((0)) NOT NULL,
    [PayBalance]             BIT             CONSTRAINT [DF_Org_tbOrg_PayBalance] DEFAULT ((1)) NOT NULL,
    [NumberOfEmployees]      INT             CONSTRAINT [DF_Org_tb_NumberOfEmployees] DEFAULT ((0)) NOT NULL,
    [CompanyNumber]          NVARCHAR (20)   NULL,
    [VatNumber]              NVARCHAR (50)   NULL,
    [EUJurisdiction]         BIT             CONSTRAINT [DF_Org_tb_EUJurisdiction] DEFAULT ((0)) NOT NULL,
    [BusinessDescription]    NTEXT           NULL,
    [Logo]                   IMAGE           NULL,
    [InsertedBy]             NVARCHAR (50)   CONSTRAINT [DF_Org_tb_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]             DATETIME        CONSTRAINT [DF_Org_tb_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]              NVARCHAR (50)   CONSTRAINT [DF_Org_tb_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]              DATETIME        CONSTRAINT [DF_Org_tb_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]                 ROWVERSION      NOT NULL,
    [TransmitStatusCode]     SMALLINT        CONSTRAINT [DF_Org_tbOrg_TransmitStatusCode] DEFAULT ((0)) NOT NULL,
    [OpeningBalance]         DECIMAL (18, 5) CONSTRAINT [DF_Org_tb_OpeningBalance] DEFAULT ((0)) NOT NULL,
    [Turnover]               DECIMAL (18, 5) CONSTRAINT [DF_Org_tb_Turnover] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Org_tbOrg] PRIMARY KEY NONCLUSTERED ([AccountCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Org_tb_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Org_tb_Org_tbAddress] FOREIGN KEY ([AddressCode]) REFERENCES [Org].[tbAddress] ([AddressCode]) NOT FOR REPLICATION,
    CONSTRAINT [FK_Org_tbOrg_tbTransmitStatus] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Org].[tbTransmitStatus] ([TransmitStatusCode]),
    CONSTRAINT [FK_Org_tbOrg_tbStatus] FOREIGN KEY ([OrganisationStatusCode]) REFERENCES [Org].[tbStatus] ([OrganisationStatusCode]),
    CONSTRAINT [FK_Org_tbOrg_tbType] FOREIGN KEY ([OrganisationTypeCode]) REFERENCES [Org].[tbType] ([OrganisationTypeCode])
);


GO
ALTER TABLE [Org].[tbOrg] NOCHECK CONSTRAINT [FK_Org_tb_Org_tbAddress];


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Org_tb_AccountName]
    ON [Org].[tbOrg]([AccountName] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tb_AccountSource]
    ON [Org].[tbOrg]([AccountSource] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tb_AreaCode]
    ON [Org].[tbOrg]([AreaCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tbOrg_OpeningBalance]
    ON [Org].[tbOrg]([AccountCode] ASC)
    INCLUDE([OpeningBalance]);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tb_OrganisationStatusCode]
    ON [Org].[tbOrg]([OrganisationStatusCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Org_tb_Status_AccountCode]
    ON [Org].[tbOrg]([OrganisationStatusCode] ASC, [AccountName] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tb_OrganisationTypeCode]
    ON [Org].[tbOrg]([OrganisationTypeCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Org_tb_PaymentTerms]
    ON [Org].[tbOrg]([PaymentTerms] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_tbOrg_tb_AccountCode]
    ON [Org].[tbOrg]([AccountCode] ASC)
    INCLUDE([AccountName]);


GO
CREATE   TRIGGER Org.Org_tbOrg_TriggerUpdate 
   ON  Org.tbOrg
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(AccountCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1);
			ROLLBACK TRANSACTION;
			END
		ELSE
			BEGIN
			UPDATE Org.tbOrg
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Org.tbOrg INNER JOIN inserted AS i ON tbOrg.AccountCode = i.AccountCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
