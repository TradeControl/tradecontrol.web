CREATE TABLE [Cash].[tbCategory] (
    [CategoryCode]     NVARCHAR (10) NOT NULL,
    [Category]         NVARCHAR (50) NOT NULL,
    [CategoryTypeCode] SMALLINT      CONSTRAINT [DF_Cash_tbCategory_CategoryTypeCode] DEFAULT ((1)) NOT NULL,
    [CashModeCode]     SMALLINT      CONSTRAINT [DF_Cash_tbCategory_CashModeCode] DEFAULT ((1)) NULL,
    [CashTypeCode]     SMALLINT      CONSTRAINT [DF_Cash_tbCategory_CashTypeCode] DEFAULT ((0)) NULL,
    [DisplayOrder]     SMALLINT      CONSTRAINT [DF_Cash_tbCategory_DisplayOrder] DEFAULT ((0)) NOT NULL,
    [IsEnabled]        SMALLINT      CONSTRAINT [DF_Cash_tbCategory_IsEnabled] DEFAULT ((1)) NOT NULL,
    [InsertedBy]       NVARCHAR (50) CONSTRAINT [DF_Cash_tbCategory_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]       DATETIME      CONSTRAINT [DF_Cash_tbCategory_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]        NVARCHAR (50) CONSTRAINT [DF_Cash_tbCategory_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]        DATETIME      CONSTRAINT [DF_Cash_tbCategory_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]           ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbCategory] PRIMARY KEY CLUSTERED ([CategoryCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Cash_tbCategory_Cash_tbCategoryType] FOREIGN KEY ([CategoryTypeCode]) REFERENCES [Cash].[tbCategoryType] ([CategoryTypeCode]),
    CONSTRAINT [FK_Cash_tbCategory_Cash_tbMode] FOREIGN KEY ([CashModeCode]) REFERENCES [Cash].[tbMode] ([CashModeCode]),
    CONSTRAINT [FK_Cash_tbCategory_Cash_tbType] FOREIGN KEY ([CashTypeCode]) REFERENCES [Cash].[tbType] ([CashTypeCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbCategory_DisplayOrder]
    ON [Cash].[tbCategory]([DisplayOrder] ASC, [Category] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbCategory_IsEnabled_Category]
    ON [Cash].[tbCategory]([IsEnabled] ASC, [Category] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbCategory_IsEnabled_CategoryCode]
    ON [Cash].[tbCategory]([IsEnabled] ASC, [CategoryCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbCategory_Name]
    ON [Cash].[tbCategory]([Category] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbCategory_TypeCategory]
    ON [Cash].[tbCategory]([CategoryTypeCode] ASC, [Category] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbCategory_TypeOrderCategory]
    ON [Cash].[tbCategory]([CategoryTypeCode] ASC, [DisplayOrder] ASC, [Category] ASC) WITH (FILLFACTOR = 90);


GO
CREATE TRIGGER Cash.Cash_tbCategory_TriggerUpdate 
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
