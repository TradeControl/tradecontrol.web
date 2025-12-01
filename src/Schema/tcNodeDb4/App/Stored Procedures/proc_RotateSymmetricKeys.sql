CREATE PROCEDURE App.proc_RotateSymmetricKeys
    @NewKey VARBINARY(32) = NULL,           -- optional: provide a new AES-256 key
    @NewIV  VARBINARY(16) = NULL,           -- optional: provide a new IV
    @OldKeyInput VARBINARY(32) = NULL,      -- optional: caller-supplied current key for verification
    @OldIVInput  VARBINARY(16) = NULL,      -- optional: caller-supplied current IV for verification
    @OldKey VARBINARY(32) = NULL OUTPUT,    -- returns previous key (NULL if none)
    @OldIV  VARBINARY(16) = NULL OUTPUT     -- returns previous IV (NULL if none)
AS
SET NOCOUNT, XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    IF NOT EXISTS (SELECT 1 FROM [App].[tbOptions])
    BEGIN
        RAISERROR ('proc_RotateSymmetricKeys failed: tbOptions not initialised', 16, 1);
        ROLLBACK TRAN;
        RETURN 1; 
    END

    SELECT @OldKey = SymmetricKey, @OldIV = SymmetricIV
    FROM [App].[tbOptions];

    IF @OldKey IS NOT NULL
    BEGIN
        IF @OldKeyInput IS NULL
        BEGIN
            RAISERROR('proc_RotateSymmetricKeys failed: existing symmetric key present - caller must supply the current key for verification', 16, 1);
            ROLLBACK TRAN;
            RETURN 2; 
        END

        IF @OldKeyInput IS NOT NULL AND @OldKey <> @OldKeyInput
        BEGIN
            RAISERROR('proc_RotateSymmetricKeys failed: supplied old key does not match current key', 16, 1);
            ROLLBACK TRAN;
            RETURN 3; 
        END
    END

    IF @OldIV IS NOT NULL AND @OldIVInput IS NOT NULL AND @OldIV <> @OldIVInput
    BEGIN
        RAISERROR('proc_RotateSymmetricKeys failed: supplied old IV does not match current IV', 16, 1);
        ROLLBACK TRAN;
        RETURN 4; 
    END

    IF @NewKey IS NULL
        SET @NewKey = CRYPT_GEN_RANDOM(32); -- AES-256

    IF @NewIV IS NULL
        SET @NewIV = CRYPT_GEN_RANDOM(16);  -- AES block size IV

    UPDATE [App].[tbOptions]
    SET SymmetricKey = @NewKey,
        SymmetricIV  = @NewIV;

    COMMIT TRAN;
    RETURN 0; 
END TRY
BEGIN CATCH
    EXEC App.proc_ErrorLog;
    RETURN 100; 
END CATCH;
GO