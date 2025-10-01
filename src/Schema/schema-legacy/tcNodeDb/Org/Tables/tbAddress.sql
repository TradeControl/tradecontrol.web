CREATE TABLE [Org].[tbAddress] (
    [AddressCode] NVARCHAR (15) NOT NULL,
    [AccountCode] NVARCHAR (10) NOT NULL,
    [Address]     NTEXT         NOT NULL,
    [InsertedBy]  NVARCHAR (50) CONSTRAINT [DF_Org_tbAddress_InsertedBy] DEFAULT (suser_sname()) NOT NULL,
    [InsertedOn]  DATETIME      CONSTRAINT [DF_Org_tbAddress_InsertedOn] DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]   NVARCHAR (50) CONSTRAINT [DF_Org_tbAddress_UpdatedBy] DEFAULT (suser_sname()) NOT NULL,
    [UpdatedOn]   DATETIME      CONSTRAINT [DF_Org_tbAddress_UpdatedOn] DEFAULT (getdate()) NOT NULL,
    [RowVer]      ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Org_tbAddress] PRIMARY KEY CLUSTERED ([AddressCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Org_tbAddress_Org_tb] FOREIGN KEY ([AccountCode]) REFERENCES [Org].[tbOrg] ([AccountCode]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Org_tbAddress]
    ON [Org].[tbAddress]([AccountCode] ASC, [AddressCode] ASC) WITH (FILLFACTOR = 90);


GO
CREATE   TRIGGER Org.Org_tbAddress_TriggerInsert
ON Org.tbAddress 
FOR INSERT
AS
	SET NOCOUNT ON;
	BEGIN TRY
		If EXISTS(SELECT     Org.tbOrg.AddressCode, Org.tbOrg.AccountCode
				  FROM         Org.tbOrg INNER JOIN
										inserted AS i ON Org.tbOrg.AccountCode = i.AccountCode
				  WHERE     ( Org.tbOrg.AddressCode IS NULL))
			BEGIN
			UPDATE Org.tbOrg
			SET AddressCode = i.AddressCode
			FROM         Org.tbOrg INNER JOIN
										inserted AS i ON Org.tbOrg.AccountCode = i.AccountCode
				  WHERE     ( Org.tbOrg.AddressCode IS NULL)
			END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH


GO
CREATE   TRIGGER Org.Org_tbAddress_TriggerUpdate 
   ON  Org.tbAddress
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE Org.tbAddress
		SET UpdatedBy = SUSER_SNAME(), UpdatedOn = CURRENT_TIMESTAMP
		FROM Org.tbAddress INNER JOIN inserted AS i ON tbAddress.AddressCode = i.AddressCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
END
