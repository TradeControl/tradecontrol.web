CREATE TABLE [Org].[tbAccount] (
    [CashAccountCode] NVARCHAR (10)   NOT NULL,
    [AccountCode]     NVARCHAR (10)   NOT NULL,
    [CashAccountName] NVARCHAR (50)   NOT NULL,
    [SortCode]        NVARCHAR (10)   NULL,
    [AccountNumber]   NVARCHAR (20)   NULL,
    [CashCode]        NVARCHAR (50)   NULL,
    [AccountClosed]   BIT             CONSTRAINT [DF_Org_tbAccount_AccountClosed] DEFAULT ((0)) NOT NULL,
    [InsertedBy]      NVARCHAR (50)   CONSTRAINT [DF_Org_tbAccount_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME        CONSTRAINT [DF_Org_tbAccount_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)   CONSTRAINT [DF_Org_tbAccount_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME        CONSTRAINT [DF_Org_tbAccount_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    [OpeningBalance]  DECIMAL (18, 5) CONSTRAINT [DF_Org_tbAccount_OpeningBalance] DEFAULT ((0)) NOT NULL,
    [CurrentBalance]  DECIMAL (18, 5) CONSTRAINT [DF_Org_tbAccount_CurrentBalance] DEFAULT ((0)) NOT NULL,
    [CoinTypeCode]    SMALLINT        CONSTRAINT [DF_Org_tbAccount_CoinTypeCode] DEFAULT ((2)) NOT NULL,
    [AccountTypeCode] SMALLINT        CONSTRAINT [DF_Org_tbAccount_AccountTypeCode] DEFAULT ((0)) NOT NULL,
    [LiquidityLevel]  SMALLINT        CONSTRAINT [DF_Org_tbAccount_LiquidityLevel] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Org_tbAccount] PRIMARY KEY CLUSTERED ([CashAccountCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Org_tbAccount_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]),
    CONSTRAINT [FK_Org_tbAccount_Cash_tbCoinType] FOREIGN KEY ([CoinTypeCode]) REFERENCES [Cash].[tbCoinType] ([CoinTypeCode]),
    CONSTRAINT [FK_Org_tbAccount_Org_tb] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Org_tbAccount_Org_tbAccountType] FOREIGN KEY ([AccountTypeCode]) REFERENCES [Org].[tbAccountType] ([AccountTypeCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Org_tbAccount]
    ON [Org].[tbAccount]([AccountCode] ASC, [CashAccountCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_tbAccount_AccountTypeCode]
    ON [Org].[tbAccount]([AccountTypeCode] ASC, [LiquidityLevel] DESC, [CashAccountCode] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Org_tbAccount_CashAccountName]
    ON [Org].[tbAccount]([CashAccountName] ASC);


GO
CREATE TRIGGER Org.Org_tbAccount_TriggerUpdate 
   ON  Org.tbAccount
   AFTER UPDATE, INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
	DECLARE @Msg NVARCHAR(MAX);

		IF EXISTS (SELECT * FROM inserted i WHERE App.fnParsePrimaryKey(CashAccountCode) = 0)
			BEGIN		
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 2004;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
		ELSE IF EXISTS (SELECT * FROM inserted i JOIN Cash.tbCode c ON i.CashCode = c.CashCode WHERE AccountTypeCode = 1)
			BEGIN
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 3015;
			RAISERROR (@Msg, 10, 1)
			ROLLBACK
			END
		ELSE
			BEGIN
			IF UPDATE(OpeningBalance)
			BEGIN
			
				WITH i AS
				(
					SELECT * FROM inserted WHERE AccountTypeCode = 0
				)
				UPDATE Org.tbAccount
				SET CurrentBalance = balance.CurrentBalance
				FROM Org.tbAccount 
					INNER JOIN i ON tbAccount.CashAccountCode = i.CashAccountCode
					INNER JOIN Cash.vwAccountRebuild balance ON balance.CashAccountCode = i.CashAccountCode;

				WITH i AS
				(
					SELECT * FROM inserted WHERE AccountTypeCode = 0
				)		
				UPDATE Org.tbAccount
				SET CurrentBalance = Org.tbAccount.OpeningBalance
				FROM  Cash.vwAccountRebuild 
					RIGHT OUTER JOIN Org.tbAccount ON Cash.vwAccountRebuild.CashAccountCode = Org.tbAccount.CashAccountCode
					JOIN i ON i.CashAccountCode = Org.tbAccount.CashAccountCode
				WHERE   (Cash.vwAccountRebuild.CashAccountCode IS NULL);
			END

			UPDATE Org.tbAccount
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Org.tbAccount INNER JOIN inserted AS i ON tbAccount.CashAccountCode = i.CashAccountCode;
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
