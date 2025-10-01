CREATE TABLE [Cash].[tbTx] (
    [TxNumber]       INT             IDENTITY (1, 1) NOT NULL,
    [PaymentAddress] NVARCHAR (42)   NOT NULL,
    [TxId]           NVARCHAR (64)   NOT NULL,
    [TransactedOn]   DATETIME        CONSTRAINT [DF_Cash_tbTx_TransactedOn] DEFAULT (getdate()) NOT NULL,
    [TxStatusCode]   SMALLINT        CONSTRAINT [DF_Cash_tbTx_TxStatusCode] DEFAULT ((0)) NOT NULL,
    [MoneyIn]        DECIMAL (18, 5) CONSTRAINT [DF_Cash_tbTx_MoneyIn] DEFAULT ((0)) NOT NULL,
    [MoneyOut]       DECIMAL (18, 5) CONSTRAINT [DF_Cash_tbTx_MoneyOut] DEFAULT ((0)) NOT NULL,
    [Confirmations]  INT             CONSTRAINT [DF_Cash_tbTx_Confirmations] DEFAULT ((0)) NOT NULL,
    [TxMessage]      NVARCHAR (50)   NULL,
    [InsertedBy]     NVARCHAR (50)   CONSTRAINT [DF_Cash_tbTx_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    CONSTRAINT [PK_Cash_tbTx] PRIMARY KEY CLUSTERED ([TxNumber] ASC),
    CONSTRAINT [FK_Cash_tbTx_Cash_tbChange] FOREIGN KEY ([PaymentAddress]) REFERENCES [Cash].[tbChange] ([PaymentAddress]) ON DELETE CASCADE,
    CONSTRAINT [FK_Cash_tbTx_Cash_tbTxStatus] FOREIGN KEY ([TxStatusCode]) REFERENCES [Cash].[tbTxStatus] ([TxStatusCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbTx_PaymentAddress]
    ON [Cash].[tbTx]([PaymentAddress] ASC, [TxId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbTx_TxStatusCode]
    ON [Cash].[tbTx]([TxStatusCode] ASC, [TransactedOn] ASC);


GO
CREATE   TRIGGER Cash.Cash_tbTx_Trigger
   ON  Cash.tbTx
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

		WITH payment AS
		(
			SELECT PaymentAddress
			FROM inserted tx
		), balance_base AS
		(
			SELECT tx.PaymentAddress, tx.TxStatusCode, SUM(CASE WHEN tx.TxStatusCode > 0 THEN tx.MoneyIn ELSE 0 END) Balance
			FROM Cash.tbTx tx JOIN payment ON tx.PaymentAddress = payment.PaymentAddress
			GROUP BY tx.PaymentAddress, tx.TxStatusCode
		), tx_balance AS
		(
			SELECT  PaymentAddress, MIN(TxStatusCode) TxStatusCode, SUM(Balance) Balance
			FROM balance_base
			GROUP BY PaymentAddress
		)
		UPDATE change
		SET	ChangeStatusCode =	CASE
									WHEN tx_balance.TxStatusCode = 2 THEN tx_balance.TxStatusCode
									WHEN tx_balance.Balance > 0 THEN 1
									ELSE tx_balance.TxStatusCode
								END 
		FROM tx_balance
			JOIN Cash.tbChange change ON tx_balance.PaymentAddress = change.PaymentAddress;		

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END

GO
CREATE   TRIGGER [Cash].[Cash_tbTx_Trigger_Delete]
   ON  [Cash].[tbTx]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY

		WITH payment AS
		(
			SELECT PaymentAddress, 0 Balance
			FROM deleted tx
		), balance_base AS
		(
			SELECT tx.PaymentAddress, tx.TxStatusCode, SUM(CASE WHEN tx.TxStatusCode > 0 THEN tx.MoneyIn ELSE 0 END) Balance
			FROM Cash.tbTx tx JOIN payment ON tx.PaymentAddress = payment.PaymentAddress
			GROUP BY tx.PaymentAddress, tx.TxStatusCode
		), tx_balance AS
		(
			SELECT  PaymentAddress, MIN(TxStatusCode) TxStatusCode, SUM(Balance) Balance
			FROM balance_base
			GROUP BY PaymentAddress
		)
		UPDATE change
		SET	ChangeStatusCode =	CASE
									WHEN tx_balance.TxStatusCode = 2 THEN tx_balance.TxStatusCode
									WHEN tx_balance.Balance > 0 THEN 1
									ELSE tx_balance.TxStatusCode
								END 
		FROM tx_balance
			JOIN Cash.tbChange change ON tx_balance.PaymentAddress = change.PaymentAddress;	

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
