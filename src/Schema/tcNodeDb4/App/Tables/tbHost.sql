
CREATE TABLE [App].[tbHost] (
    [HostId]          INT            IDENTITY (1, 1) NOT NULL,
    [HostDescription] NVARCHAR (50)  NOT NULL,
    [EmailAddress]    VARCHAR (256)  NOT NULL,
    [EmailPassword]   NVARCHAR (50)  NOT NULL,
    [IsSmtpAuth]      BIT            CONSTRAINT [DF_App_tbHost_IsSmtpAuth] DEFAULT ((1)) NOT NULL,
    [HostName]        NVARCHAR (256) NOT NULL,
    [HostPort]        INT            NULL,
    [InsertedBy]      NVARCHAR (50)  CONSTRAINT [DF_App_tbHost_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME       CONSTRAINT [DF_App_tbHost_InsertedOn] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_App_tbHost] PRIMARY KEY NONCLUSTERED ([HostId] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_App_tbHost_HostDescription]
    ON [App].[tbHost]([HostDescription] ASC);
