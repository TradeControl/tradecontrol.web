CREATE TABLE [Project].[tbChangeLog] (
    [ProjectCode]           NVARCHAR (20)   NOT NULL,
    [LogId]              INT             IDENTITY (1, 1) NOT NULL,
    [ChangedOn]          DATETIME        CONSTRAINT [DF_Project_tbChangeLog_ChangedOn] DEFAULT (dateadd(millisecond,datepart(millisecond,getdate())*(-1),getdate())) NOT NULL,
    [TransmitStatusCode] SMALLINT        CONSTRAINT [DF_Project_tbChangeLog_TransmissionStatusCode] DEFAULT ((0)) NOT NULL,
    [SubjectCode]        NVARCHAR (10)   NOT NULL,
    [ObjectCode]       NVARCHAR (50)   NOT NULL,
    [ProjectStatusCode]     SMALLINT        NOT NULL,
    [ActionOn]           DATETIME        NOT NULL,
    [CashCode]           NVARCHAR (50)   NULL,
    [TaxCode]            NVARCHAR (10)   NULL,
    [UpdatedBy]          NVARCHAR (50)   CONSTRAINT [DF_Project_tbChangeLog_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [RowVer]             ROWVERSION      NOT NULL,
    [Quantity]           DECIMAL (18, 4) CONSTRAINT [DF_Project_tbChangeLog_Quantity] DEFAULT ((0)) NOT NULL,
    [UnitCharge]         DECIMAL (18, 7) CONSTRAINT [DF_Project_tbChangeLog_UnitCharge] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Project_tbChangeLog] PRIMARY KEY CLUSTERED ([ProjectCode] ASC, [LogId] DESC),
    CONSTRAINT [FK_Project_tbChangeLog_TrasmitStatusCode] FOREIGN KEY ([TransmitStatusCode]) REFERENCES [Subject].[tbTransmitStatus] ([TransmitStatusCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Project_tbChangeLog_LogId]
    ON [Project].[tbChangeLog]([LogId] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbChangeLog_ChangedOn]
    ON [Project].[tbChangeLog]([ChangedOn] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Project_tbChangeLog_TransmitStatus]
    ON [Project].[tbChangeLog]([TransmitStatusCode] ASC, [ChangedOn] ASC);

