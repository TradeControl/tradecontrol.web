CREATE TABLE [Org].[tbTransmitStatus] (
    [TransmitStatusCode] SMALLINT      NOT NULL,
    [TransmitStatus]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_App_tbTransmitStatus] PRIMARY KEY CLUSTERED ([TransmitStatusCode] ASC)
);

