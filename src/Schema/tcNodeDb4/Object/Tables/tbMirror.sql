CREATE TABLE [Object].[tbMirror] (
    [ObjectCode]       NVARCHAR (50) NOT NULL,
    [SubjectCode]        NVARCHAR (10) NOT NULL,
    [AllocationCode]     NVARCHAR (50) NOT NULL,
    [TransmitStatusCode] SMALLINT      CONSTRAINT [DF_Object_tbMirror_TransmitStatusCode] DEFAULT ((0)) NOT NULL,
    [InsertedBy]         NVARCHAR (50) CONSTRAINT [DF_Object_tbMirror_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]         DATETIME      CONSTRAINT [DF_Object_tbMirror_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]          NVARCHAR (50) CONSTRAINT [DF_Object_tbMirror_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]          DATETIME      CONSTRAINT [DF_Object_tbMirror_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]             ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Object_tbMirror] PRIMARY KEY CLUSTERED ([ObjectCode] ASC, [SubjectCode] ASC, [AllocationCode] ASC),
    CONSTRAINT [FK_Object_tbMirror_tbObject] FOREIGN KEY ([ObjectCode]) REFERENCES [Object].[tbObject] ([ObjectCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Object_tbMirror_tbSubject] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Object_tbMirror_tbTransmitStatus] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Object_tbMirror_AllocationCode]
    ON [Object].[tbMirror]([SubjectCode] ASC, [AllocationCode] ASC)
    INCLUDE([ObjectCode]);


GO
CREATE NONCLUSTERED INDEX [IX_Object_tbMirror_TransmitStatusCode]
    ON [Object].[tbMirror]([TransmitStatusCode] ASC, [AllocationCode] ASC);


GO
CREATE   TRIGGER [Object].Object_tbMirror_Trigger_Insert
ON Object.tbMirror
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE mirror
		SET TransmitStatusCode = Subject.TransmitStatusCode
		FROM Object.tbMirror mirror 
			JOIN inserted ON mirror.SubjectCode = inserted.SubjectCode AND mirror.ObjectCode = inserted.ObjectCode
			JOIN Subject.tbSubject Subject ON inserted.SubjectCode = Subject.SubjectCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE   TRIGGER [Object].Object_tbMirror_Trigger_Update
ON Object.tbMirror
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
			FROM Object.tbMirror mirror 
				JOIN inserted ON mirror.SubjectCode = inserted.SubjectCode AND mirror.ObjectCode = inserted.ObjectCode
				JOIN Subject.tbSubject Subject ON inserted.SubjectCode = Subject.SubjectCode
			WHERE inserted.TransmitStatusCode <> 1;
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
