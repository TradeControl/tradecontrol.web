CREATE TABLE [App].[tbExecutionStatus] (
    [ExecutionStatusCode] SMALLINT      NOT NULL,
    [ExecutionStatus]     NVARCHAR (25) NOT NULL,
    CONSTRAINT [PK_App_tbExecutionStatus] PRIMARY KEY CLUSTERED ([ExecutionStatusCode] ASC)
);
GO
