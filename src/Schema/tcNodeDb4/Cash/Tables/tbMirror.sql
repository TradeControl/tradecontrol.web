CREATE TABLE [Cash].[tbMirror] (
    [CashCode]           NVARCHAR (50) NOT NULL,
    [SubjectCode]        NVARCHAR (10) NOT NULL,
    [ChargeCode]         NVARCHAR (50) NOT NULL,
    [TransmitStatusCode] SMALLINT      CONSTRAINT [DF_Cash_tbMirror_TransmitStatusCode] DEFAULT ((0)) NOT NULL,
    [InsertedBy]         NVARCHAR (50) CONSTRAINT [DF_Cash_tbMirror_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]         DATETIME      CONSTRAINT [DF_Cash_tbMirror_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]          NVARCHAR (50) CONSTRAINT [DF_Cash_tbMirror_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]          DATETIME      CONSTRAINT [DF_Cash_tbMirror_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]             ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbMirror] PRIMARY KEY CLUSTERED ([CashCode] ASC, [SubjectCode] ASC, [ChargeCode] ASC),
    CONSTRAINT [FK_Cash_tbMirror_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]),
    CONSTRAINT [FK_Cash_tbMirror_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]),
    CONSTRAINT [FK_Cash_tbMirror_tbTransmitStatus] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbMirror_ChargeCode]
    ON [Cash].[tbMirror]([SubjectCode] ASC, [ChargeCode] ASC)
    INCLUDE([CashCode]);


GO
CREATE NONCLUSTERED INDEX [IX_Cash_tbMirror_TransmitStatusCode]
    ON [Cash].[tbMirror]([TransmitStatusCode] ASC, [ChargeCode] ASC);


GO
CREATE   TRIGGER [Cash].Cash_tbMirror_Trigger_Insert
ON Cash.tbMirror
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE mirror
		SET TransmitStatusCode = Subject.TransmitStatusCode
		FROM Cash.tbMirror mirror 
			JOIN inserted ON mirror.SubjectCode = inserted.SubjectCode AND mirror.CashCode = inserted.CashCode
			JOIN Subject.tbSubject Subject ON inserted.SubjectCode = Subject.SubjectCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE   TRIGGER [Cash].Cash_tbMirror_Trigger_Update
ON Cash.tbMirror
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		IF NOT UPDATE(TransmitStatusCode)
		BEGIN
			UPDATE mirror
			SET 
				TransmitStatusCode = CASE WHEN Subject.TransmitStatusCode = 1 THEN 2 ELSE 0 END,
				UpdatedBy = SUSER_NAME(),
				UpdatedOn = CURRENT_TIMESTAMP
			FROM Cash.tbMirror mirror 
				JOIN inserted ON mirror.SubjectCode = inserted.SubjectCode AND mirror.CashCode = inserted.CashCode
				JOIN Subject.tbSubject Subject ON inserted.SubjectCode = Subject.SubjectCode
			WHERE inserted.TransmitStatusCode <> 1;
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
