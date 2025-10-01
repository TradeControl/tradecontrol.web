CREATE TABLE [Activity].[tbActivity] (
    [ActivityCode]        NVARCHAR (50)   NOT NULL,
    [TaskStatusCode]      SMALLINT        CONSTRAINT [DF_Activity_tbActivity_TaskStatusCode] DEFAULT ((1)) NOT NULL,
    [UnitOfMeasure]       NVARCHAR (15)   NOT NULL,
    [CashCode]            NVARCHAR (50)   NULL,
    [Printed]             BIT             CONSTRAINT [DF_Activity_tbActivity_Printed] DEFAULT ((0)) NOT NULL,
    [RegisterName]        NVARCHAR (50)   NULL,
    [InsertedBy]          NVARCHAR (50)   CONSTRAINT [DF_Activity_tbActivity_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]          DATETIME        CONSTRAINT [DF_Activity_tbActivity_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]           NVARCHAR (50)   CONSTRAINT [DF_Activity_tbActivity_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]           DATETIME        CONSTRAINT [DF_Activity_tbActivity_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]              ROWVERSION      NOT NULL,
    [ActivityDescription] NVARCHAR (100)  NULL,
    [UnitCharge]          DECIMAL (18, 7) CONSTRAINT [DF_Activity_tbActivity_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Activity_tbActivityCode] PRIMARY KEY NONCLUSTERED ([ActivityCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Activity_tbActivity_App_tbRegister] FOREIGN KEY ([RegisterName]) REFERENCES [App].[tbRegister] ([RegisterName]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Activity_tbActivity_App_tbUom] FOREIGN KEY ([UnitOfMeasure]) REFERENCES [App].[tbUom] ([UnitOfMeasure]),
    CONSTRAINT [FK_Activity_tbActivity_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON UPDATE CASCADE
);


GO

/*  TRIGGERS ****/
CREATE   TRIGGER Activity.Activity_tbActivity_TriggerUpdate
   ON  Activity.tbActivity
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(ActivityCode) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
		ELSE
			BEGIN
			UPDATE Activity.tbActivity
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Activity.tbActivity INNER JOIN inserted AS i ON tbActivity.ActivityCode = i.ActivityCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
