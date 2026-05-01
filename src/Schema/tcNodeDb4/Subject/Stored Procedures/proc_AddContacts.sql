CREATE PROCEDURE Subject.proc_AddContacts
(
    @Xml nvarchar(max)
)
AS
    SET NOCOUNT, XACT_ABORT ON;

    BEGIN TRY
        DECLARE @Contacts xml = TRY_CAST(@Xml AS xml);

        IF @Contacts IS NULL
            RETURN;

        DECLARE
            @SubjectCode nvarchar(50),
            @ContactName nvarchar(100);

        DECLARE contact_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT
                X.Contact.value('(SubjectCode/text())[1]', 'nvarchar(50)') AS SubjectCode,
                X.Contact.value('(ContactName/text())[1]', 'nvarchar(100)') AS ContactName
            FROM @Contacts.nodes('/Contacts/Contact') AS X(Contact);

        OPEN contact_cursor;

        FETCH NEXT FROM contact_cursor
        INTO @SubjectCode, @ContactName;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC Subject.proc_AddContact
                @SubjectCode = @SubjectCode,
                @ContactName = @ContactName;

            FETCH NEXT FROM contact_cursor
            INTO @SubjectCode, @ContactName;
        END

        CLOSE contact_cursor;
        DEALLOCATE contact_cursor;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
