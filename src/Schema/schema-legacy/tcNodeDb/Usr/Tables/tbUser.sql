CREATE TABLE [Usr].[tbUser] (
    [UserId]          NVARCHAR (10)  NOT NULL,
    [UserName]        NVARCHAR (50)  NOT NULL,
    [LogonName]       NVARCHAR (50)  CONSTRAINT [DF_Usr_tb_LogonName] DEFAULT (suser_sname()) NOT NULL,
    [CalendarCode]    NVARCHAR (10)  NULL,
    [PhoneNumber]     NVARCHAR (50)  NULL,
    [MobileNumber]    NVARCHAR (50)  NULL,
    [EmailAddress]    NVARCHAR (255) NULL,
    [Address]         NTEXT          NULL,
    [Avatar]          IMAGE          NULL,
    [Signature]       IMAGE          NULL,
    [IsAdministrator] BIT            CONSTRAINT [DF_Usr_tbUser_IsAdministrator] DEFAULT ((0)) NOT NULL,
    [IsEnabled]       SMALLINT       CONSTRAINT [DF_Usr_tbUser_IsEnabled] DEFAULT ((1)) NOT NULL,
    [NextTaskNumber]  INT            CONSTRAINT [DF_Usr_tb_NextTaskNumber] DEFAULT ((1)) NOT NULL,
    [InsertedBy]      NVARCHAR (50)  CONSTRAINT [DF_Usr_tb_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]      DATETIME       CONSTRAINT [DF_Usr_tb_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (50)  CONSTRAINT [DF_Usr_tb_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]       DATETIME       CONSTRAINT [DF_Usr_tb_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]          ROWVERSION     NOT NULL,
    [MenuViewCode]    SMALLINT       CONSTRAINT [DF_Usr_tbUser_MenuViewCode] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Usr_tbUser] PRIMARY KEY CLUSTERED ([UserId] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Usr_tb_App_tbCalendar] FOREIGN KEY ([CalendarCode]) REFERENCES [App].[tbCalendar] ([CalendarCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Usr_tbMenu_Usr_tbUser] FOREIGN KEY ([MenuViewCode]) REFERENCES [Usr].[tbMenuView] ([MenuViewCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usr_tbUser_IsEnabled_LogonName]
    ON [Usr].[tbUser]([IsEnabled] ASC, [LogonName] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usr_tbUser_IsEnabled_UserName]
    ON [Usr].[tbUser]([IsEnabled] ASC, [UserName] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usr_tbUser_LogonName]
    ON [Usr].[tbUser]([LogonName] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Usr_tbUser_UserName]
    ON [Usr].[tbUser]([UserName] ASC);


GO
CREATE TRIGGER Usr.Usr_tbUser_TriggerUpdate 
   ON  Usr.tbUser
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF NOT UPDATE(UserName)
		BEGIN
			UPDATE Usr.tbUser
			SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
			FROM Usr.tbUser INNER JOIN inserted AS i ON tbUser.UserId = i.UserId;
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
