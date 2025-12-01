CREATE TABLE [Cash].[tbTxReference] (
    [TxNumber]     INT           NOT NULL,
    [TxStatusCode] SMALLINT      CONSTRAINT [DF_Cash_tbTxReference_TxStatusCode] DEFAULT ((0)) NOT NULL,
    [PaymentCode]  NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbTxReference] PRIMARY KEY CLUSTERED ([TxNumber] ASC, [TxStatusCode] ASC),
    CONSTRAINT [FK_Cash_tbTxReference_Cash_tbPayment] FOREIGN KEY ([PaymentCode]) REFERENCES [Cash].[tbPayment] ([PaymentCode]),
    CONSTRAINT [FK_Cash_tbTxReference_Cash_tbTx] FOREIGN KEY ([TxNumber]) REFERENCES [Cash].[tbTx] ([TxNumber]),
    CONSTRAINT [FK_Cash_tbTxReference_Cash_tbTxStatus] FOREIGN KEY ([TxStatusCode]) REFERENCES [Cash].[tbTxStatus] ([TxStatusCode])
);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbTxReference_PaymentCode]
    ON [Cash].[tbTxReference]([PaymentCode] ASC, [TxNumber] ASC);


GO
CREATE   TRIGGER Cash.Cash_tbTxReference_TriggerDelete
   ON  Cash.tbTxReference
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Cash.tbTx
		SET 
			TxStatusCode = CASE change.ChangeTypeCode WHEN 0 THEN 0 ELSE 1 END
		FROM deleted 
			JOIN Cash.tbTx tx ON deleted.TxNumber = tx.TxNumber
			JOIN Cash.tbChange change ON tx.PaymentAddress = change.PaymentAddress
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
