CREATE TABLE [Usr].[tbMenuUser] (
    [UserId] NVARCHAR (10) NOT NULL,
    [MenuId] SMALLINT      NOT NULL,
    [RowVer] ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Usr_tbMenuUser] PRIMARY KEY CLUSTERED ([UserId] ASC, [MenuId] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Usr_tbMenu_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Usr_tbMenu_Usr_tbMenu] FOREIGN KEY ([MenuId]) REFERENCES [Usr].[tbMenu] ([MenuId])
);

