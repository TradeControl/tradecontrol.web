CREATE TABLE [Activity].[tbMirror] (
    [ActivityCode]       NVARCHAR (50) NOT NULL,
    [AccountCode]        NVARCHAR (10) NOT NULL,
    [AllocationCode]     NVARCHAR (50) NOT NULL,
    [TransmitStatusCode] SMALLINT      CONSTRAINT [DF_Activity_tbMirror_TransmitStatusCode] DEFAULT ((0)) NOT NULL,
    [InsertedBy]         NVARCHAR (50) CONSTRAINT [DF_Activity_tbMirror_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]         DATETIME      CONSTRAINT [DF_Activity_tbMirror_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]          NVARCHAR (50) CONSTRAINT [DF_Activity_tbMirror_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]          DATETIME      CONSTRAINT [DF_Activity_tbMirror_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]             ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Activity_tbMirror] PRIMARY KEY CLUSTERED ([ActivityCode] ASC, [AccountCode] ASC, [AllocationCode] ASC),
    CONSTRAINT [FK_Activity_tbMirror_tbActivity] FOREIGN KEY ([ActivityCode]) REFERENCES [Activity].[tbActivity] ([ActivityCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Activity_tbMirror_tbOrg] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Activity_tbMirror_tbTransmitStatus] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Org].[tbTransmitStatus] ([TransmitStatusCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Activity_tbMirror_AllocationCode]
    ON [Activity].[tbMirror]([AccountCode] ASC, [AllocationCode] ASC)
    INCLUDE([ActivityCode]);


GO
CREATE NONCLUSTERED INDEX [IX_Activity_tbMirror_TransmitStatusCode]
    ON [Activity].[tbMirror]([TransmitStatusCode] ASC, [AllocationCode] ASC);


GO
CREATE   TRIGGER [Activity].Activity_tbMirror_Trigger_Insert
ON Activity.tbMirror
FOR INSERT
AS
	SET NOCOUNT ON;

	BEGIN TRY

		UPDATE mirror
		SET TransmitStatusCode = org.TransmitStatusCode
		FROM Activity.tbMirror mirror 
			JOIN inserted ON mirror.AccountCode = inserted.AccountCode AND mirror.ActivityCode = inserted.ActivityCode
			JOIN Org.tbOrg org ON inserted.AccountCode = org.AccountCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

GO
CREATE   TRIGGER [Activity].Activity_tbMirror_Trigger_Update
ON Activity.tbMirror
FOR UPDATE
AS
	SET NOCOUNT ON;

	BEGIN TRY
		IF NOT UPDATE(TransmitStatusCode)
		BEGIN
			UPDATE mirror
			SET 
				TransmitStatusCode = CASE WHEN org.TransmitStatusCode = 1 THEN 2 ELSE 0 END,
				UpdatedBy = SUSER_NAME(),
				UpdatedOn = CURRENT_TIMESTAMP
			FROM Activity.tbMirror mirror 
				JOIN inserted ON mirror.AccountCode = inserted.AccountCode AND mirror.ActivityCode = inserted.ActivityCode
				JOIN Org.tbOrg org ON inserted.AccountCode = org.AccountCode
			WHERE inserted.TransmitStatusCode <> 1;
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
