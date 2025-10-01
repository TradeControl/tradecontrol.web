CREATE TABLE [Object].[tbObject] (
    [ObjectCode]        NVARCHAR (50)   NOT NULL,
    [ProjectStatusCode]      SMALLINT        CONSTRAINT [DF_Object_tbObject_ProjectStatusCode] DEFAULT ((1)) NOT NULL,
    [UnitOfMeasure]       NVARCHAR (15)   NOT NULL,
    [CashCode]            NVARCHAR (50)   NULL,
    [Printed]             BIT             CONSTRAINT [DF_Object_tbObject_Printed] DEFAULT ((0)) NOT NULL,
    [RegisterName]        NVARCHAR (50)   NULL,
    [InsertedBy]          NVARCHAR (50)   CONSTRAINT [DF_Object_tbObject_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]          DATETIME        CONSTRAINT [DF_Object_tbObject_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]           NVARCHAR (50)   CONSTRAINT [DF_Object_tbObject_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]           DATETIME        CONSTRAINT [DF_Object_tbObject_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]              ROWVERSION      NOT NULL,
    [ObjectDescription] NVARCHAR (100)  NULL,
    [UnitCharge]          DECIMAL (18, 7) CONSTRAINT [DF_Object_tbObject_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Object_tbObjectCode] PRIMARY KEY NONCLUSTERED ([ObjectCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Object_tbObject_App_tbRegister] FOREIGN KEY ([RegisterName]) REFERENCES [App].[tbRegister] ([RegisterName]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Object_tbObject_App_tbUom] FOREIGN KEY ([UnitOfMeasure]) REFERENCES [App].[tbUom] ([UnitOfMeasure]),
    CONSTRAINT [FK_Object_tbObject_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON UPDATE CASCADE
);


GO

/*  TRIGGERS ****/
CREATE   TRIGGER Object.Object_tbObject_TriggerUpdate
   ON  Object.tbObject
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(ObjectCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
		ELSE
			BEGIN
			UPDATE Object.tbObject
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Object.tbObject INNER JOIN inserted AS i ON tbObject.ObjectCode = i.ObjectCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
