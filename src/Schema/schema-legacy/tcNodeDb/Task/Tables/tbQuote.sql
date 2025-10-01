CREATE TABLE [Task].[tbQuote] (
    [TaskCode]        NVARCHAR (20)   NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Task_tbQuote_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Task_tbQuote_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Task_tbQuote_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Task_tbQuote_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Quantity]        DECIMAL (18, 4) CONSTRAINT [DF_Task_tbQuote_Quantity] DEFAULT ((0)) NOT NULL,
    [RunOnQuantity]   DECIMAL (18, 4) CONSTRAINT [DF_Task_tbQuote_RunOnQuantity] DEFAULT ((0)) NOT NULL,
    [RunBackQuantity] DECIMAL (18, 4) CONSTRAINT [DF_Task_tbQuote_RunBackQuantity] DEFAULT ((0)) NOT NULL,
    [TotalPrice]      DECIMAL (18, 5) CONSTRAINT [DF_Task_tbQuote_TotalPrice] DEFAULT ((0)) NOT NULL,
    [RunOnPrice]      DECIMAL (18, 5) CONSTRAINT [DF_Task_tbQuote_RunOnPrice] DEFAULT ((0)) NOT NULL,
    [RunBackPrice]    DECIMAL (18, 5) CONSTRAINT [DF_Task_tbQuote_RunBackPrice] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Task_tbQuote] PRIMARY KEY CLUSTERED ([TaskCode] ASC, [Quantity] ASC),
    CONSTRAINT [FK_Task_tbQuote_Task_tb] FOREIGN KEY ([TaskCode]) REFERENCES [Task].[tbTask] ([TaskCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE   TRIGGER Task.Task_tbQuote_TriggerUpdate 
   ON  Task.tbQuote
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Task.tbQuote
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Task.tbQuote INNER JOIN inserted AS i ON tbQuote.TaskCode = i.TaskCode AND tbQuote.Quantity = i.Quantity;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
