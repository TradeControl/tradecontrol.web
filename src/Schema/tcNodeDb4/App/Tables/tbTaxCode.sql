CREATE TABLE [App].[tbTaxCode] (
    [TaxCode]        NVARCHAR (10)   NOT NULL,
    [TaxDescription] NVARCHAR (50)   NOT NULL,
    [TaxTypeCode]    SMALLINT        CONSTRAINT [DF_App_tbTaxCode_TaxTypeCode] DEFAULT ((2)) NOT NULL,
    [RoundingCode]   SMALLINT        CONSTRAINT [DF_tbTaxCode_RoundingCode] DEFAULT ((0)) NOT NULL,
    [UpdatedBy]      NVARCHAR (50)   CONSTRAINT [DF_App_tbTaxCode_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]      DATETIME        CONSTRAINT [DF_App_tbTaxCode_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]         ROWVERSION      NOT NULL,
    [TaxRate]        DECIMAL (18, 4) CONSTRAINT [DF_App_tbTaxCode_TaxRate] DEFAULT ((0)) NOT NULL,
    [Decimals]       SMALLINT        CONSTRAINT [DF_App_tbTaxCode_Decimals] DEFAULT ((2)) NOT NULL,
    CONSTRAINT [PK_App_tbTaxCode] PRIMARY KEY CLUSTERED ([TaxCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_App_tbTaxCode_App_tbRounding] FOREIGN KEY ([RoundingCode]) REFERENCES [App].[tbRounding] ([RoundingCode]),
    CONSTRAINT [FK_App_tbTaxCode_Cash_tbTaxType] FOREIGN KEY ([TaxTypeCode]) REFERENCES [Cash].[tbTaxType] ([TaxTypeCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_App_tbTaxCodeByType]
    ON [App].[tbTaxCode]([TaxTypeCode] ASC, [TaxCode] ASC) WITH (FILLFACTOR = 90);


GO

CREATE TRIGGER App.App_tbTaxCode_TriggerUpdate ON App.tbTaxCode AFTER UPDATE, INSERT
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
