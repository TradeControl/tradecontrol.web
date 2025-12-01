CREATE TABLE [App].[tbUom] (
    [UnitOfMeasure] NVARCHAR (15) NOT NULL,
    [RowVer]        ROWVERSION    NOT NULL,
    CONSTRAINT [PK_App_tbUom] PRIMARY KEY CLUSTERED ([UnitOfMeasure] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE   TRIGGER App.App_tbUom_TriggerUpdate
   ON  App.tbUom
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY	
		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(UnitOfMeasure) = 0)
			BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
