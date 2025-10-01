CREATE TABLE [Project].[tbQuote] (
    [ProjectCode]        NVARCHAR (20)   NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Project_tbQuote_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Project_tbQuote_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Project_tbQuote_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Project_tbQuote_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [Quantity]        DECIMAL (18, 4) CONSTRAINT [DF_Project_tbQuote_Quantity] DEFAULT ((0)) NOT NULL,
    [RunOnQuantity]   DECIMAL (18, 4) CONSTRAINT [DF_Project_tbQuote_RunOnQuantity] DEFAULT ((0)) NOT NULL,
    [RunBackQuantity] DECIMAL (18, 4) CONSTRAINT [DF_Project_tbQuote_RunBackQuantity] DEFAULT ((0)) NOT NULL,
    [TotalPrice]      DECIMAL (18, 5) CONSTRAINT [DF_Project_tbQuote_TotalPrice] DEFAULT ((0)) NOT NULL,
    [RunOnPrice]      DECIMAL (18, 5) CONSTRAINT [DF_Project_tbQuote_RunOnPrice] DEFAULT ((0)) NOT NULL,
    [RunBackPrice]    DECIMAL (18, 5) CONSTRAINT [DF_Project_tbQuote_RunBackPrice] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Project_tbQuote] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [Quantity] ASC),
    CONSTRAINT [FK_Project_tbQuote_Project_tb] FOREIGN KEY ([ProjectCode]) REFERENCES [Project].[tbProject] ([ProjectCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE   TRIGGER Project.Project_tbQuote_TriggerUpdate 
   ON  Project.tbQuote
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Project.tbQuote
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Project.tbQuote INNER JOIN inserted AS i ON tbQuote.ProjectCode = i.ProjectCode AND tbQuote.Quantity = i.Quantity;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
