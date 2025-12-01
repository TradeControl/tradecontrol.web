SET NOCOUNT ON;

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

-- b. Source table
IF OBJECT_ID('tempdb..#SourceTableScripts') IS NOT NULL DROP TABLE #SourceTableScripts;
CREATE TABLE #SourceTableScripts (
    SchemaName NVARCHAR(128),
    TableName NVARCHAR(128),
    SelectStatement NVARCHAR(MAX)
);

-- Populate #SourceTableScripts
DECLARE @SourceDb NVARCHAR(128) = 'tcNodeDb3';
DECLARE @TableName NVARCHAR(128);
DECLARE @SchemaName NVARCHAR(128);
DECLARE @ColList NVARCHAR(MAX);
DECLARE @SelectStmt NVARCHAR(MAX);

DECLARE SourceCursor CURSOR FOR
SELECT s.name, t.name
FROM [tcNodeDb3].sys.tables t
JOIN [tcNodeDb3].sys.schemas s ON t.schema_id = s.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;

OPEN SourceCursor;
FETCH NEXT FROM SourceCursor INTO @SchemaName, @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @ColList = STRING_AGG(QUOTENAME(c.name), ', ')
    FROM [tcNodeDb3].sys.columns c
    WHERE c.object_id = OBJECT_ID(QUOTENAME(@SourceDb) + '.' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName))
      AND c.system_type_id NOT IN (189, 80);

    SET @SelectStmt = 'SELECT ' + @ColList + ' FROM ' + QUOTENAME(@SourceDb) + '.' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName);

    INSERT INTO #SourceTableScripts (SchemaName, TableName, SelectStatement)
    VALUES (@SchemaName, @TableName, @SelectStmt);

    FETCH NEXT FROM SourceCursor INTO @SchemaName, @TableName;
END

CLOSE SourceCursor;
DEALLOCATE SourceCursor;

-- c. Destination table
IF OBJECT_ID('tempdb..#DestinationTableScripts') IS NOT NULL DROP TABLE #DestinationTableScripts;
CREATE TABLE #DestinationTableScripts (
    SchemaName NVARCHAR(128),
    TableName NVARCHAR(128),
    InsertStatement NVARCHAR(MAX),
    IsIdentity BIT
);

DECLARE @DestinationDb NVARCHAR(128) = 'tcNodeDb4';
DECLARE @InsertStmt NVARCHAR(MAX);
DECLARE @HasIdentity BIT;

DECLARE DestCursor CURSOR FOR
SELECT s.name, t.name
FROM [tcNodeDb4].sys.tables t
JOIN [tcNodeDb4].sys.schemas s ON t.schema_id = s.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;

OPEN DestCursor;
FETCH NEXT FROM DestCursor INTO @SchemaName, @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @ColList = STRING_AGG(QUOTENAME(c.name), ', ')
    FROM [tcNodeDb4].sys.columns c
    WHERE c.object_id = OBJECT_ID(QUOTENAME(@DestinationDb) + '.' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName))
      AND c.system_type_id NOT IN (189, 80);

    SELECT @HasIdentity = COUNT(*)
    FROM [tcNodeDb4].sys.columns c
    WHERE c.object_id = OBJECT_ID(QUOTENAME(@DestinationDb) + '.' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName))
      AND c.is_identity = 1;

    SET @InsertStmt = '';
    IF @HasIdentity = 1
        SET @InsertStmt = @InsertStmt + 'SET IDENTITY_INSERT ' + QUOTENAME(@DestinationDb) + '.' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + ' ON;' + CHAR(13) + CHAR(10);

    SET @InsertStmt = @InsertStmt +
        'INSERT INTO ' + QUOTENAME(@DestinationDb) + '.' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) +
        ' (' + @ColList + ') VALUES (...);';

    IF @HasIdentity = 1
        SET @InsertStmt = @InsertStmt + CHAR(13) + CHAR(10) + 'SET IDENTITY_INSERT ' + QUOTENAME(@DestinationDb) + '.' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + ' OFF;';

    INSERT INTO #DestinationTableScripts (SchemaName, TableName, InsertStatement, IsIdentity)
    VALUES (@SchemaName, @TableName, @InsertStmt, @HasIdentity);

    FETCH NEXT FROM DestCursor INTO @SchemaName, @TableName;
END

CLOSE DestCursor;
DEALLOCATE DestCursor;

-- d. Disable all foreign key constraints in the destination database
DECLARE @DestSchema NVARCHAR(128), @DestTable NVARCHAR(128), @InsertStatement2 NVARCHAR(MAX), @IsIdentity2 BIT;
DECLARE @SourceSchema2 NVARCHAR(128), @SourceTable2 NVARCHAR(128), @SelectStatement2 NVARCHAR(MAX);
DECLARE @ColList2 NVARCHAR(MAX), @SQL NVARCHAR(MAX);

DECLARE DestProcessCursor CURSOR FOR
SELECT SchemaName, TableName, InsertStatement, IsIdentity
FROM #DestinationTableScripts;

OPEN DestProcessCursor;
FETCH NEXT FROM DestProcessCursor INTO @DestSchema, @DestTable, @InsertStatement2, @IsIdentity2;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Use LEFT JOIN to prefer mapping, fallback to direct match
    SELECT TOP 1
        @SourceSchema2 = s.SchemaName,
        @SourceTable2 = s.TableName,
        @SelectStatement2 = s.SelectStatement
    FROM #SourceTableScripts s
    LEFT JOIN #SchemaNameMapping m ON s.TableName = m.V3Name
    WHERE s.SchemaName = @DestSchema
      AND (
            (@DestTable = m.V4Name) -- mapped name
         OR (@DestTable = s.TableName AND m.V4Name IS NULL) -- direct match if no mapping
      );

    IF @SourceTable2 IS NOT NULL
    BEGIN
        SET @ColList2 = SUBSTRING(
            @InsertStatement2, 
            CHARINDEX('(', @InsertStatement2) + 1, 
            CHARINDEX(')', @InsertStatement2) - CHARINDEX('(', @InsertStatement2) - 1
        );

        SET @SQL = '';
        IF @IsIdentity2 = 1
            SET @SQL = @SQL + 'SET IDENTITY_INSERT tcNodeDb4.[' + @DestSchema + '].[' + @DestTable + '] ON;' + CHAR(13) + CHAR(10);

        SET @SQL = @SQL +
            'INSERT INTO tcNodeDb4.[' + @DestSchema + '].[' + @DestTable + '] (' + @ColList2 + ')' + CHAR(13) + CHAR(10) +
            REPLACE(@SelectStatement2, 'SELECT ', 'SELECT ') + ';' + CHAR(13) + CHAR(10);

        IF @IsIdentity2 = 1
            SET @SQL = @SQL + 'SET IDENTITY_INSERT tcNodeDb4.[' + @DestSchema + '].[' + @DestTable + '] OFF;' + CHAR(13) + CHAR(10);

        SET @SQL += CHAR(13) + CHAR(10);
        PRINT @SQL;
        -- EXEC sp_executesql @SQL; -- Uncomment to execute
    END

    -- Reset for next iteration
    SET @SourceSchema2 = NULL;
    SET @SourceTable2 = NULL;
    SET @SelectStatement2 = NULL;

    FETCH NEXT FROM DestProcessCursor INTO @DestSchema, @DestTable, @InsertStatement2, @IsIdentity2;
END

CLOSE DestProcessCursor;
DEALLOCATE DestProcessCursor;

-- f. Re-enable all foreign key constraints in the destination database
DECLARE @EnableConstraints NVARCHAR(MAX) = N'';
SELECT @EnableConstraints = CAST(STRING_AGG(
    CAST('ALTER TABLE [' + s.name + '].[' + t.name + '] WITH CHECK CHECK CONSTRAINT [' + fk.name + '];' AS NVARCHAR(MAX)),
    CHAR(13) + CHAR(10)
) AS NVARCHAR(MAX))
FROM tcNodeDb4.sys.foreign_keys fk
JOIN tcNodeDb4.sys.tables t ON fk.parent_object_id = t.object_id
JOIN tcNodeDb4.sys.schemas s ON t.schema_id = s.schema_id;
IF @EnableConstraints IS NOT NULL EXEC sp_executesql @EnableConstraints;

-- Drop all temp tables
DROP TABLE #SchemaNameMapping;
DROP TABLE #SourceTableScripts;
DROP TABLE #DestinationTableScripts;
