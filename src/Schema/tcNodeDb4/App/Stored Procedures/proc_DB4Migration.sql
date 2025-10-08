CREATE PROCEDURE App.proc_DB4Migration
(
    @SourceDb NVARCHAR(128) = 'tcNodeDb3',
    @DestinationDb sysname = NULL,
    @ExecSQL BIT = 0
)
AS
SET NOCOUNT, XACT_ABORT ON
BEGIN TRY

    IF @DestinationDb IS NULL
    SET @DestinationDb = DB_NAME();

    IF @ExecSQL = 1
        BEGIN TRANSACTION;
    ELSE
    BEGIN
        PRINT 'SET XACT_ABORT, NOCOUNT ON'
        PRINT 'BEGIN TRY'
        PRINT CHAR(9) + 'BEGIN TRAN'
    END

    -- a. Mapping table
    IF OBJECT_ID('tempdb..#SchemaNameMapping') IS NOT NULL DROP TABLE #SchemaNameMapping;
    CREATE TABLE #SchemaNameMapping (
        V3Name NVARCHAR(128),
        V4Name NVARCHAR(128)
    );

    INSERT INTO #SchemaNameMapping (V3Name, V4Name) VALUES
    ('Activity', 'Object'),
    ('Organisation', 'Subject'),
    ('Org', 'Subject'),
    ('Task', 'Project'),
    ('tbMode', 'tbPolarity'),
    ('CashMode', 'CashPolarity'),
    ('AccountCode', 'SubjectCode'),
    ('AccountName', 'SubjectName'),
    ('AccountSource', 'SubjectSource'),
    ('DefaultAccountCode', 'DefaultSubjectCode'),
    ('AccountLookup', 'SubjectLookup'),
    ('CashAccountCode', 'AccountCode'),
    ('CashAccountName', 'AccountName');

    -- Add explicit mappings for the three pillars
    INSERT INTO #SchemaNameMapping (V3Name, V4Name) VALUES
    ('Org.tbOrg', 'Subject.tbSubject'),
    ('Activity.tbActivity', 'Object.tbObject'),
    ('Task.tbTask', 'Project.tbProject'),
    ('Invoice.tbTask', 'Invoice.tbProject');

    -- b. Source table
    IF OBJECT_ID('tempdb..#SourceTableScripts') IS NOT NULL DROP TABLE #SourceTableScripts;
    CREATE TABLE #SourceTableScripts (
        SchemaName NVARCHAR(128),
        TableName NVARCHAR(128),
        SelectStatement NVARCHAR(MAX)
    );

    DECLARE @SourceSql NVARCHAR(MAX) = N'
    INSERT INTO #SourceTableScripts (SchemaName, TableName, SelectStatement)
    SELECT s.name, t.name,
        ''SELECT '' + STRING_AGG(QUOTENAME(c.name), '', '') + '' FROM [' + @SourceDb + '].['' + s.name + ''].['' + t.name + '']''
    FROM [' + @SourceDb + '].sys.tables t
    JOIN [' + @SourceDb + '].sys.schemas s ON t.schema_id = s.schema_id
    JOIN [' + @SourceDb + '].sys.columns c ON t.object_id = c.object_id
    WHERE t.is_ms_shipped = 0
      AND c.is_computed = 0
      AND c.system_type_id NOT IN (189) -- 189=rowversion/timestamp
    GROUP BY s.name, t.name;
    ';
    SET @SourceSql = REPLACE(@SourceSql, '@SourceDb', @SourceDb);
    EXEC(@SourceSql);

    -- c. Destination table
    IF OBJECT_ID('tempdb..#DestinationTableScripts') IS NOT NULL DROP TABLE #DestinationTableScripts;
    CREATE TABLE #DestinationTableScripts (
        SchemaName NVARCHAR(128),
        TableName NVARCHAR(128),
        InsertStatement NVARCHAR(MAX),
        IsIdentity BIT
    );

    DECLARE @DestSql NVARCHAR(MAX) = N'
    INSERT INTO #DestinationTableScripts (SchemaName, TableName, InsertStatement, IsIdentity)
    SELECT s.name, t.name,
        CASE WHEN SUM(CASE WHEN c.is_identity = 1 THEN 1 ELSE 0 END) > 0
            THEN ''SET IDENTITY_INSERT [' + @DestinationDb + '].['' + s.name + ''].['' + t.name + ''] ON;'' + CHAR(13) + CHAR(10)
            ELSE ''''
        END +
        ''INSERT INTO [' + @DestinationDb + '].['' + s.name + ''].['' + t.name + ''] ('' +
        STRING_AGG(QUOTENAME(c.name), '', '') + '') VALUES (...);'' +
        CASE WHEN SUM(CASE WHEN c.is_identity = 1 THEN 1 ELSE 0 END) > 0
            THEN CHAR(13) + CHAR(10) + ''SET IDENTITY_INSERT [' + @DestinationDb + '].['' + s.name + ''].['' + t.name + ''] OFF;''
            ELSE ''''
        END,
        CASE WHEN SUM(CASE WHEN c.is_identity = 1 THEN 1 ELSE 0 END) > 0 THEN 1 ELSE 0 END
    FROM [' + @DestinationDb + '].sys.tables t
    JOIN [' + @DestinationDb + '].sys.schemas s ON t.schema_id = s.schema_id
    JOIN [' + @DestinationDb + '].sys.columns c ON t.object_id = c.object_id
    WHERE t.is_ms_shipped = 0
      AND c.is_computed = 0
      AND c.system_type_id NOT IN (189) -- 189=rowversion/timestamp
    GROUP BY s.name, t.name;
    ';
    SET @DestSql = REPLACE(@DestSql, '@DestinationDb', @DestinationDb);
    EXEC(@DestSql);

    -- d. Disable all foreign key constraints and triggers in the destination database
    DECLARE @DisableSql NVARCHAR(MAX) = N'
    DECLARE curDisable CURSOR FOR
    SELECT ''ALTER TABLE [' + @DestinationDb + '].['' + s.name + ''].['' + t.name + ''] NOCHECK CONSTRAINT ['' + fk.name + ''];''
    FROM [' + @DestinationDb + '].sys.foreign_keys fk
    JOIN [' + @DestinationDb + '].sys.tables t ON fk.parent_object_id = t.object_id
    JOIN [' + @DestinationDb + '].sys.schemas s ON t.schema_id = s.schema_id;

    OPEN curDisable
    DECLARE @disableStmt NVARCHAR(MAX)
    FETCH NEXT FROM curDisable INTO @disableStmt
    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT @disableStmt
        IF ' + CAST(@ExecSQL AS NVARCHAR(1)) + N' = 1 EXEC sp_executesql @disableStmt
        FETCH NEXT FROM curDisable INTO @disableStmt
    END
    CLOSE curDisable
    DEALLOCATE curDisable
    ';

    EXEC(@DisableSql);

    PRINT '';

    -- Disable all triggers
    SET @DisableSql = N'
    DECLARE curDisable CURSOR FOR
        SELECT ''DISABLE TRIGGER ['' + trg.name + ''] ON [' + @DestinationDb + '].['' + s.name + ''].['' + t.name + ''];''
        FROM [' + @DestinationDb + '].sys.triggers trg
        JOIN [' + @DestinationDb + '].sys.tables t ON trg.parent_id = t.object_id
        JOIN [' + @DestinationDb + '].sys.schemas s ON t.schema_id = s.schema_id
        WHERE trg.parent_class = 1;

    DECLARE @disableStmt NVARCHAR(4000);
    OPEN curDisable;
    FETCH NEXT FROM curDisable INTO @disableStmt;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT @disableStmt;
        IF ' + CAST(@ExecSQL AS NVARCHAR(1)) + N' = 1 EXEC sp_executesql @disableStmt;
        FETCH NEXT FROM curDisable INTO @disableStmt;
    END
    CLOSE curDisable;
    DEALLOCATE curDisable;
    ';

    EXEC(@DisableSql);
    
    PRINT '';

    -- e. Read through the destination table and process each line
    DECLARE @DestSchema NVARCHAR(128), @DestTable NVARCHAR(128), @InsertStatement2 NVARCHAR(MAX), @IsIdentity2 BIT;
    DECLARE @SourceSchema2 NVARCHAR(128), @SourceTable2 NVARCHAR(128), @SelectStatement2 NVARCHAR(MAX);
    DECLARE @ColList2 NVARCHAR(MAX), @SQL NVARCHAR(MAX);

    DECLARE DestProcessCursor CURSOR FOR
    SELECT SchemaName, TableName, InsertStatement, IsIdentity
    FROM #DestinationTableScripts ds

    OPEN DestProcessCursor;
    FETCH NEXT FROM DestProcessCursor INTO @DestSchema, @DestTable, @InsertStatement2, @IsIdentity2;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        WITH cand AS
        (
            SELECT 
                    s.SchemaName ss,
                    s.TableName st,
                    s.SelectStatement,
                    sm.V3Name,
                    sm.V4Name
            FROM #SourceTableScripts s
                LEFT JOIN #SchemaNameMapping sm 
                    ON (sm.V3Name = CONCAT(s.SchemaName, '.', s.TableName) AND sm.V4Name = CONCAT(@DestSchema, '.', @DestTable))
                        OR (sm.V3Name = s.TableName AND sm.V4Name = @DestTable)
                        OR (sm.V3Name = s.SchemaName AND sm.V4Name = @DestSchema)
        ), direct_match AS
        (
            SELECT *
            FROM cand c
            WHERE c.V3Name IS NULL AND c.ss = @DestSchema AND c.st = @DestTable
        ), composite AS
        (
            SELECT *
            FROM cand c
            WHERE c.V3Name = CONCAT(c.ss, '.', c.st) AND c.V4Name = CONCAT(@DestSchema, '.', @DestTable)
        ), table_match AS
        (
            SELECT *
            FROM cand c
            WHERE @DestTable = c.st AND c.V4Name = @DestSchema
        ), result AS
        (
            SELECT ss SchemaName, st TableName, SelectStatement FROM direct_match
            UNION
            SELECT ss SchemaName, st TableName, SelectStatement FROM composite
            UNION
            SELECT ss SchemaName, st TableName, SelectStatement FROM table_match
        )
        SELECT
            @SourceSchema2 = SchemaName,
            @SourceTable2 = TableName,
            @SelectStatement2 = SelectStatement
        FROM result;

        IF @SourceTable2 IS NOT NULL
        BEGIN
            SET @ColList2 = SUBSTRING(
                @InsertStatement2, 
                CHARINDEX('(', @InsertStatement2) + 1, 
                CHARINDEX(')', @InsertStatement2) - CHARINDEX('(', @InsertStatement2) - 1
            );

            SET @SQL = '';
            SET @SQL = @SQL + 'DELETE FROM [' + @DestinationDb + '].[' + @DestSchema + '].[' + @DestTable + '];' + CHAR(13) + CHAR(10);

            IF @IsIdentity2 = 1
                SET @SQL = @SQL + 'SET IDENTITY_INSERT [' + @DestinationDb + '].[' + @DestSchema + '].[' + @DestTable + '] ON;' + CHAR(13) + CHAR(10);

            SET @SQL = @SQL +
                'INSERT INTO [' + @DestinationDb + '].[' + @DestSchema + '].[' + @DestTable + '] (' + @ColList2 + ')' + CHAR(13) + CHAR(10) +
                REPLACE(@SelectStatement2, 'SELECT ', 'SELECT ') + ';' + CHAR(13) + CHAR(10);

            IF @IsIdentity2 = 1
                SET @SQL = @SQL + 'SET IDENTITY_INSERT [' + @DestinationDb + '].[' + @DestSchema + '].[' + @DestTable + '] OFF;' + CHAR(13) + CHAR(10);

            SET @SQL += CHAR(13) + CHAR(10);

            PRINT @SQL;
            IF @ExecSQL = 1 
                EXEC(@SQL);
        END

        SET @SourceSchema2 = NULL;
        SET @SourceTable2 = NULL;
        SET @SelectStatement2 = NULL;

        FETCH NEXT FROM DestProcessCursor INTO @DestSchema, @DestTable, @InsertStatement2, @IsIdentity2;
    END

    CLOSE DestProcessCursor;
    DEALLOCATE DestProcessCursor;

    -- f. Re-enable all foreign key constraints and triggers in the destination database
    DECLARE @EnableSql NVARCHAR(MAX) = N'
    DECLARE curEnable CURSOR FOR
    SELECT ''ALTER TABLE [' + @DestinationDb + '].['' + s.name + ''].['' + t.name + ''] WITH CHECK CHECK CONSTRAINT ['' + fk.name + ''];''
    FROM [' + @DestinationDb + '].sys.foreign_keys fk
    JOIN [' + @DestinationDb + '].sys.tables t ON fk.parent_object_id = t.object_id
    JOIN [' + @DestinationDb + '].sys.schemas s ON t.schema_id = s.schema_id;

    OPEN curEnable
    DECLARE @enableStmt NVARCHAR(MAX)
    FETCH NEXT FROM curEnable INTO @enableStmt
    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT @enableStmt
        IF ' + CAST(@ExecSQL AS NVARCHAR(1)) + N' = 1 EXEC sp_executesql @enableStmt
        FETCH NEXT FROM curEnable INTO @enableStmt
    END
    CLOSE curEnable
    DEALLOCATE curEnable
    ';

    EXEC(@EnableSql);

    -- Enable all triggers
    SET @EnableSql = N'
    DECLARE curEnable CURSOR FOR
        SELECT ''ENABLE TRIGGER ['' + trg.name + ''] ON [' + @DestinationDb + '].['' + s.name + ''].['' + t.name + ''];''
        FROM [' + @DestinationDb + '].sys.triggers trg
        JOIN [' + @DestinationDb + '].sys.tables t ON trg.parent_id = t.object_id
        JOIN [' + @DestinationDb + '].sys.schemas s ON t.schema_id = s.schema_id
        WHERE trg.parent_class = 1;

    DECLARE @enableStmt NVARCHAR(4000);
    OPEN curEnable;
    FETCH NEXT FROM curEnable INTO @enableStmt;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT @enableStmt;
        IF ' + CAST(@ExecSQL AS NVARCHAR(1)) + N' = 1 EXEC sp_executesql @enableStmt;
        FETCH NEXT FROM curEnable INTO @enableStmt;
    END
    CLOSE curEnable;
    DEALLOCATE curEnable;
    ';

    EXEC(@EnableSql);
        
    DROP TABLE #SchemaNameMapping;
    DROP TABLE #SourceTableScripts;
    DROP TABLE #DestinationTableScripts;

    IF @ExecSQL = 1
        COMMIT TRANSACTION;
    ELSE
    BEGIN
        PRINT CHAR(9) + 'COMMIT TRAN'
        PRINT 'END TRY'
        PRINT 'BEGIN CATCH'
        PRINT CHAR(9) + 'PRINT CONCAT(ERROR_SEVERITY(), '' '', ERROR_STATE(), '' '', ERROR_MESSAGE());'
        PRINT CHAR(9) + CHAR(9) + 'ROLLBACK TRAN'
        PRINT 'END CATCH'
    END

END TRY
BEGIN CATCH
    EXEC App.proc_ErrorLog;
END CATCH