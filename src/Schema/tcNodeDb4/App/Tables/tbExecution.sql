CREATE TABLE [App].[tbExecution] (
    [ExecutionCode]       NVARCHAR (20)  NOT NULL,
    [ExecutionType]       NVARCHAR (50)  NOT NULL,
    [ExecutionStatusCode] SMALLINT       CONSTRAINT [DF_App_tbExecution_ExecutionStatusCode] DEFAULT ((0)) NOT NULL,
    [QueuedBy]            NVARCHAR (10)  NULL,
    [QueuedOn]            DATETIME       CONSTRAINT [DF_App_tbExecution_QueuedOn] DEFAULT (getdate()) NOT NULL,
    [StartedOn]           DATETIME       NULL,
    [CompletedOn]         DATETIME       NULL,
    [Arguments]           NVARCHAR (MAX) NULL,
    [ProgressMessage]     NVARCHAR (255) NULL,
    [ErrorMessage]        NVARCHAR (MAX) NULL,
    [RowVer]              ROWVERSION     NOT NULL,
    CONSTRAINT [PK_App_tbExecution] PRIMARY KEY CLUSTERED ([ExecutionCode] ASC),
    CONSTRAINT [FK_App_tbExecution_App_tbExecutionStatus]
        FOREIGN KEY ([ExecutionStatusCode]) REFERENCES [App].[tbExecutionStatus] ([ExecutionStatusCode]),
    CONSTRAINT [FK_App_tbExecution_Usr_tbUser]
        FOREIGN KEY ([QueuedBy]) REFERENCES [Usr].[tbUser] ([UserId]) NOT FOR REPLICATION 
);
GO

CREATE NONCLUSTERED INDEX [IX_App_tbExecution_ExecutionStatusCode_QueuedOn]
    ON [App].[tbExecution]([ExecutionStatusCode] ASC, [QueuedOn] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_App_tbExecution_QueuedBy_QueuedOn]
    ON [App].[tbExecution]([QueuedBy] ASC, [QueuedOn] DESC);
GO
