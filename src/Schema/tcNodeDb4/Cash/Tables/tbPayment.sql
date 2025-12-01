CREATE TABLE [Cash].[tbPayment] (
    [PaymentCode]       NVARCHAR (20)   NOT NULL,
    [UserId]            NVARCHAR (10)   NOT NULL,
    [PaymentStatusCode] SMALLINT        CONSTRAINT [DF_Cash_tbPayment_PaymentStatusCode] DEFAULT ((0)) NOT NULL,
    [SubjectCode]       NVARCHAR (10)   NOT NULL,
    [AccountCode]   NVARCHAR (10)   NOT NULL,
    [CashCode]          NVARCHAR (50)   NULL,
    [TaxCode]           NVARCHAR (10)   NULL,
    [PaidOn]            DATETIME        CONSTRAINT [DF_Cash_tbPayment_PaidOn] DEFAULT (CONVERT([date],getdate())) NOT NULL,
    [PaidInValue]       DECIMAL (18, 5) CONSTRAINT [DF_Cash_tbPayment_PaidInValue] DEFAULT ((0)) NOT NULL,
    [PaidOutValue]      DECIMAL (18, 5) CONSTRAINT [DF_Cash_tbPayment_PaidOutValue] DEFAULT ((0)) NOT NULL,
    [PaymentReference]  NVARCHAR (50)   NULL,
    [InsertedBy]        NVARCHAR (50)   CONSTRAINT [DF_Cash_tbPayment_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]        DATETIME        CONSTRAINT [DF_Cash_tbPayment_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]         NVARCHAR (50)   CONSTRAINT [DF_Cash_tbPayment_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]         DATETIME        CONSTRAINT [DF_Cash_tbPayment_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [IsProfitAndLoss]   BIT             CONSTRAINT [DF_Cash_tbPayment_IsProfitAndLoss] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Cash_tbPayment] PRIMARY KEY CLUSTERED ([PaymentCode] ASC),
    CONSTRAINT [FK_Cash_tbPayment_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]),
    CONSTRAINT [FK_Cash_tbPayment_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Cash_tbPayment_Cash_tbPaymentStatus] FOREIGN KEY ([PaymentStatusCode]) REFERENCES [Cash].[tbPaymentStatus] ([PaymentStatusCode]),
    CONSTRAINT [FK_Cash_tbPayment_Subject_tbAccount] FOREIGN KEY ([AccountCode]) REFERENCES [Subject].[tbAccount] ([AccountCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Cash_tbPayment_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]),
    CONSTRAINT [FK_Cash_tbPayment_Usr_tbUser] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment]
    ON [Cash].[tbPayment]([PaymentReference] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_AccountCode]
    ON [Cash].[tbPayment]([SubjectCode] ASC, [PaidOn] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_CashAccountCode]
    ON [Cash].[tbPayment]([AccountCode] ASC, [PaidOn] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_CashCode]
    ON [Cash].[tbPayment]([CashCode] ASC, [PaidOn] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_PaymentCode_Status]
    ON [Cash].[tbPayment]([SubjectCode] ASC, [PaymentStatusCode] ASC, [PaymentCode] ASC)
    INCLUDE([PaidInValue], [PaidOutValue]);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_PaymentCode_TaxCode]
    ON [Cash].[tbPayment]([SubjectCode] ASC, [PaymentCode] ASC, [TaxCode] ASC)
    INCLUDE([PaymentStatusCode], [PaidInValue], [PaidOutValue]);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_Status]
    ON [Cash].[tbPayment]([PaymentStatusCode] ASC)
    INCLUDE([AccountCode], [CashCode], [PaidOn], [PaidInValue], [PaidOutValue]);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_Status_AccountCode]
    ON [Cash].[tbPayment]([PaymentStatusCode] ASC, [SubjectCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbPayment_Status_CashAccount_PaidOn]
    ON [Cash].[tbPayment]([PaymentStatusCode] ASC, [AccountCode] ASC, [PaidOn] ASC)
    INCLUDE([PaymentCode], [PaidInValue], [PaidOutValue]);


GO
CREATE NONCLUSTERED INDEX [IX_tbPayment_TaxCode]
    ON [Cash].[tbPayment]([TaxCode] ASC)
    INCLUDE([PaidInValue], [PaidOutValue]);


GO
CREATE   TRIGGER Cash.Cash_tbPayment_TriggerDelete
ON Cash.tbPayment
FOR DELETE
AS
	SET NOCOUNT ON;
	BEGIN TRY

		WITH assets AS
		(
			SELECT account.AccountCode FROM deleted d
				JOIN Subject.tbAccount account ON account.AccountCode = d.AccountCode
			WHERE AccountTypeCode > 1
		), balance AS
		(
			SELECT account.AccountCode, SUM(PaidInValue + (PaidOutValue * -1)) CurrentBalance
			FROM Subject.tbAccount account
				JOIN assets ON account.AccountCode = assets.AccountCode
				JOIN Cash.tbPayment payment ON account.AccountCode = payment.AccountCode
			WHERE payment.PaymentStatusCode = 1
			GROUP BY account.AccountCode
		)
		UPDATE account
		SET CurrentBalance = balance.CurrentBalance
		FROM Subject.tbAccount account
			JOIN balance ON account.AccountCode = balance.AccountCode;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE TRIGGER Cash.Cash_tbPayment_TriggerInsert
ON Cash.tbPayment
FOR INSERT
AS
	SET NOCOUNT ON;
	BEGIN TRY

		UPDATE payment
		SET PaymentStatusCode = 2
		FROM inserted
			JOIN Cash.tbPayment payment ON inserted.PaymentCode = payment.PaymentCode
			JOIN Subject.tbAccount account ON payment.AccountCode = account.AccountCode
			JOIN Cash.tbCode ON inserted.CashCode = Cash.tbCode.CashCode 
			JOIN Cash.tbCategory category ON Cash.tbCode.CategoryCode = category.CategoryCode
		WHERE category.CashTypeCode = 2 AND inserted.PaymentStatusCode = 0 AND account.AccountTypeCode = 0;

		WITH assets AS
		(
			SELECT account.AccountCode FROM inserted i
				JOIN Subject.tbAccount account ON account.AccountCode = i.AccountCode
			WHERE AccountTypeCode = 2 AND PaymentStatusCode = 1
		), balance AS
		(
			SELECT account.AccountCode, SUM(PaidInValue + (PaidOutValue * -1)) CurrentBalance
			FROM Subject.tbAccount account
				JOIN assets ON account.AccountCode = assets.AccountCode
				JOIN Cash.tbPayment payment ON account.AccountCode = payment.AccountCode
			WHERE payment.PaymentStatusCode = 1
			GROUP BY account.AccountCode
		)
		UPDATE account
		SET CurrentBalance = balance.CurrentBalance + OpeningBalance
		FROM Subject.tbAccount account
			JOIN balance ON account.AccountCode = balance.AccountCode;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE TRIGGER Cash.Cash_tbPayment_TriggerUpdate
ON Cash.tbPayment
FOR UPDATE
AS
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Cash.tbPayment
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Cash.tbPayment INNER JOIN inserted AS i ON tbPayment.PaymentCode = i.PaymentCode;

		IF UPDATE(PaidInValue) OR UPDATE(PaidOutValue)
		BEGIN
			IF EXISTS (SELECT * FROM inserted i
					JOIN Subject.tbAccount account ON i.AccountCode = account.AccountCode AND account.AccountTypeCode = 0
				WHERE i.PaymentStatusCode = 1)
			BEGIN
				DECLARE @SubjectCode NVARCHAR(10)
				DECLARE Subject CURSOR LOCAL FOR 
					SELECT i.SubjectCode 
					FROM inserted i
						JOIN Subject.tbAccount account ON i.AccountCode = account.AccountCode AND account.AccountTypeCode = 0
					WHERE i.PaymentStatusCode = 1

				OPEN Subject
				FETCH NEXT FROM Subject INTO @SubjectCode
				WHILE (@@FETCH_STATUS = 0)
					BEGIN		
					EXEC Subject.proc_Rebuild @SubjectCode
					FETCH NEXT FROM Subject INTO @SubjectCode
				END

				CLOSE Subject
				DEALLOCATE Subject
			END
		END

		IF UPDATE(PaymentStatusCode) OR UPDATE(PaidInValue) OR UPDATE(PaidOutValue)
		BEGIN
			WITH assets AS
			(
				SELECT account.AccountCode FROM inserted i
					JOIN Subject.tbAccount account ON account.AccountCode = i.AccountCode
				WHERE AccountTypeCode = 2
			), balance AS
			(
				SELECT account.AccountCode, SUM(PaidInValue + (PaidOutValue * -1)) AS CurrentBalance
				FROM Subject.tbAccount account
					JOIN assets ON account.AccountCode = assets.AccountCode
					JOIN Cash.tbPayment payment ON account.AccountCode = payment.AccountCode
				WHERE payment.PaymentStatusCode = 1
				GROUP BY account.AccountCode
			)
			UPDATE account
			SET CurrentBalance = balance.CurrentBalance + OpeningBalance
			FROM Subject.tbAccount account
				JOIN balance ON account.AccountCode = balance.AccountCode;
		END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
