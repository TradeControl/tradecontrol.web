CREATE TABLE [Cash].[tbChange] (
    [PaymentAddress]   NVARCHAR (42)       NOT NULL,
    [AccountCode]  NVARCHAR (10)       NOT NULL,
    [HDPath]           [sys].[hierarchyid] NOT NULL,
    [ChangeTypeCode]   SMALLINT            CONSTRAINT [DF_Cash_tbChange_ChangeTypeCode] DEFAULT ((0)) NOT NULL,
    [ChangeStatusCode] SMALLINT            CONSTRAINT [DF_Cash_tbChange_ChangeStatusCode] DEFAULT ((0)) NOT NULL,
    [AddressIndex]     INT                 CONSTRAINT [DF_Cash_tbChange_AddressIndex] DEFAULT ((0)) NOT NULL,
    [Note]             NVARCHAR (256)      NULL,
    [UpdatedOn]        DATETIME            CONSTRAINT [DF_Cash_tbChange_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]        NVARCHAR (50)       CONSTRAINT [DF_Cash_tbChange_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]       DATETIME            CONSTRAINT [DF_Cash_tbChange_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [InsertedBy]       NVARCHAR (50)       CONSTRAINT [DF_Cash_tbChange_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [RowVer]           ROWVERSION          NOT NULL,
    CONSTRAINT [PK_Cash_tbChange] PRIMARY KEY CLUSTERED ([PaymentAddress] ASC),
    CONSTRAINT [FK__Cash_tbChange_Cash_tbChangeType] FOREIGN KEY ([ChangeTypeCode]) REFERENCES [Cash].[tbChangeType] ([ChangeTypeCode]),
    CONSTRAINT [FK_Cash_tbChange_Subject_tbAccountKey] FOREIGN KEY ([AccountCode], [HDPath]) REFERENCES [Subject].[tbAccountKey] ([AccountCode], [HDPath]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbChange_ChangeTypeCode]
    ON [Cash].[tbChange]([AccountCode] ASC, [HDPath] ASC, [ChangeTypeCode] ASC, [ChangeStatusCode] ASC, [AddressIndex] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbChange_ChangeStatusCode]
    ON [Cash].[tbChange]([AccountCode] ASC, [ChangeStatusCode] ASC, [AddressIndex] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbChange_UpdatedOn]
    ON [Cash].[tbChange]([AccountCode] ASC, [HDPath] ASC, [UpdatedOn] DESC);


GO
CREATE   TRIGGER Cash.Cash_tbChange_TriggerUpdate
   ON  Cash.tbChange
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Cash.tbChange
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Cash.tbChange INNER JOIN inserted AS i ON Cash.tbChange.PaymentAddress = i.PaymentAddress;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
