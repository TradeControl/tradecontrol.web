CREATE PROCEDURE Cash.proc_TaxTagMapValidate
(
    @TaxSourceCode NVARCHAR(10)
)
AS
BEGIN
    SET NOCOUNT, XACT_ABORT ON;

    DECLARE @Issues TABLE
    (
        RowNo        INT IDENTITY(1,1) NOT NULL,
        IsError      BIT NOT NULL,
        TagCode      NVARCHAR(20) NULL,
        TagName      NVARCHAR(100) NULL,
        CashCode     NVARCHAR(50) NULL,
        CategoryCode NVARCHAR(10) NULL,
        HitCount     INT NULL,
        Message      NVARCHAR(4000) NOT NULL
    );

    INSERT INTO @Issues (IsError, TagCode, TagName, CashCode, CategoryCode, HitCount, Message)
    SELECT
        v.IsError,
        v.TagCode,
        v.TagName,
        v.CashCode,
        v.CategoryCode,
        v.HitCount,
        v.Message
    FROM Cash.fnTaxTagMapValidate(@TaxSourceCode) v;

    IF NOT EXISTS (SELECT 1 FROM @Issues)
        RETURN;

    ----------------------------------------------------------------
    -- Log warnings (developers can inspect Event Log)
    ----------------------------------------------------------------
    DECLARE @Warn NVARCHAR(MAX) = N'';

    SELECT @Warn =
        @Warn
        + CASE WHEN LEN(@Warn) = 0 THEN N'' ELSE CHAR(13) + CHAR(10) END
        + CONCAT(
            COALESCE(TagCode, N''),
            CASE WHEN TagCode IS NULL THEN N'' ELSE N' ' END,
            COALESCE(TagName, N''),
            N': ',
            Message,
            CASE WHEN CashCode IS NULL THEN N'' ELSE CONCAT(N' CashCode=', CashCode) END,
            CASE WHEN CategoryCode IS NULL THEN N'' ELSE CONCAT(N' CategoryCode=', CategoryCode) END
        )
    FROM @Issues
    WHERE IsError = 0
    ORDER BY RowNo;

    IF LEN(@Warn) > 0
        EXEC App.proc_EventLog @EventMessage = @Warn, @EventTypeCode = 1;

    ----------------------------------------------------------------
    -- Throw on errors (so templates fail fast and the app sees it)
    ----------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM @Issues WHERE IsError = 1)
    BEGIN
        DECLARE @Err NVARCHAR(MAX) = CONCAT(N'MTD tag mapping errors for source ', @TaxSourceCode, N':');
        DECLARE @MaxLines INT = 20;
        DECLARE @i INT = 1;
        DECLARE @Line NVARCHAR(4000);

        WHILE (@i <= @MaxLines)
        BEGIN
            SELECT @Line =
                CONCAT(
                    CHAR(13) + CHAR(10),
                    N' - ',
                    COALESCE(TagCode, N''),
                    CASE WHEN TagCode IS NULL THEN N'' ELSE N' ' END,
                    COALESCE(TagName, N''),
                    N': ',
                    Message,
                    CASE WHEN CashCode IS NULL THEN N'' ELSE CONCAT(N' CashCode=', CashCode) END,
                    CASE WHEN CategoryCode IS NULL THEN N'' ELSE CONCAT(N' CategoryCode=', CategoryCode) END,
                    CASE WHEN HitCount IS NULL THEN N'' ELSE CONCAT(N' HitCount=', HitCount) END
                )
            FROM @Issues
            WHERE IsError = 1
              AND RowNo =
              (
                  SELECT MIN(RowNo)
                  FROM
                  (
                      SELECT RowNo, ROW_NUMBER() OVER (ORDER BY RowNo) AS rn
                      FROM @Issues
                      WHERE IsError = 1
                  ) x
                  WHERE x.rn = @i
              );

            IF @Line IS NULL
                BREAK;

            SET @Err = @Err + @Line;
            SET @i += 1;
        END

        RAISERROR ('%s', 16, 1, @Err);
    END
END;
GO
