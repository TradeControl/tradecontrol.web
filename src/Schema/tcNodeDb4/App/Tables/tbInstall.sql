CREATE TABLE [App].[tbInstall] (
    [InstallId]      INT           IDENTITY (1, 1) NOT NULL,
    [SQLDataVersion] REAL          NOT NULL,
    [SQLRelease]     INT           NOT NULL,
    [InsertedBy]     NVARCHAR (50) CONSTRAINT [DF_App_tbInstall_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]     DATETIME      CONSTRAINT [DF_App_tbInstall_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]      NVARCHAR (50) CONSTRAINT [DF_App_tbInstall_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]      DATETIME      CONSTRAINT [DF_App_tbInstall_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_App_tbInstall] PRIMARY KEY CLUSTERED ([InstallId] ASC) WITH (FILLFACTOR = 90)
);

