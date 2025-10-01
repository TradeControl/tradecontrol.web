CREATE TABLE [Web].[tbAttachment] (
    [AttachmentId]       INT            IDENTITY (1, 1) NOT NULL,
    [AttachmentFileName] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_Web_tbAttachment] PRIMARY KEY CLUSTERED ([AttachmentId] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbAttachment_AttachmentFileName]
    ON [Web].[tbAttachment]([AttachmentFileName] ASC);

