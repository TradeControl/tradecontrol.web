CREATE PROCEDURE App.proc_DatasetCreateCompanySubject
(
    @RootSubjectCode nvarchar(50),
    @SubjectName nvarchar(100),
    @SubjectTypeCode smallint,
    @TaxCode nvarchar(10),
    @PaymentTerms nvarchar(100),
    @ExpectedDays smallint = 0,
    @PaymentDays smallint = 30,
    @PayDaysFromMonthEnd bit = 0,
    @PayBalance bit = 1,
    @EUJurisdiction bit = 0,
    @SubjectCode nvarchar(50) OUTPUT
)
AS
    SET NOCOUNT, XACT_ABORT ON;

    BEGIN TRY
        EXEC Subject.proc_AddNamespace
            @RootSubjectCode = @RootSubjectCode,
            @SubjectName = @SubjectName,
            @SubjectTypeCode = @SubjectTypeCode,
            @SubjectCode = @SubjectCode OUTPUT;

        IF LEN(ISNULL(@SubjectCode, N'')) = 0
            THROW 51320, 'DatasetCreateCompanySubject: SubjectCode was not returned.', 1;

        UPDATE Subject.tbSubject
        SET
            TaxCode = @TaxCode,
            PaymentTerms = @PaymentTerms,
            ExpectedDays = @ExpectedDays,
            PaymentDays = @PaymentDays,
            PayDaysFromMonthEnd = @PayDaysFromMonthEnd,
            PayBalance = @PayBalance
        WHERE SubjectCode = @SubjectCode;

        UPDATE Subject.tbVirtual
        SET
            EUJurisdiction = @EUJurisdiction
        WHERE SubjectCode = @SubjectCode;
    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH
GO
