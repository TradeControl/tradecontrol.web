using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TradeControl.Tax.UK.Adapters.Submission.CompaniesHouse;
using TradeControl.Tax.UK.Adapters.Submission.Configuration;
using TradeControl.Tax.UK.Application.Preparation;

namespace TradeControl.Web.AppServices.TaxHub.CompaniesHouse;

internal sealed class AzureCompaniesHouseAuthorityIdentifierSource(
    IConfiguration configuration,
    IOptions<CompaniesHouseProductHostOptions> options,
    TimeProvider timeProvider) : ICompaniesHouseAuthorityIdentifierSource
{
    public async Task<CompaniesHouseAuthorityIdentifiers> AllocateAsync(
        CancellationToken cancellationToken = default)
    {
        var host = options.Value;
        if (host.AuthorityEnvironment != CompaniesHouseAuthorityEnvironment.OfficialTest
            || host.DispatchMode != CompaniesHouseDispatchMode.OfficialTestSingleSend)
            throw new InvalidOperationException("Companies House official-test identity allocation is disabled.");
        var connectionString = configuration.GetConnectionString(host.WorkflowConnectionName!);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The Companies House workflow connection is unavailable.");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await using var command = new SqlCommand("""
            DECLARE @allocated int;
            SELECT @allocated = LastAllocated
              FROM TaxHub.CompaniesHouseAuthoritySequence WITH (UPDLOCK, HOLDLOCK)
             WHERE AuthorityEnvironment = N'OfficialTest';

            IF @allocated IS NULL
            BEGIN
                SET @allocated = @floor;
                INSERT INTO TaxHub.CompaniesHouseAuthoritySequence
                    (AuthorityEnvironment, LastAllocated, UpdatedAtUtc)
                VALUES (N'OfficialTest', @allocated, SYSUTCDATETIME());
            END
            ELSE
            BEGIN
                SET @allocated = CASE WHEN @allocated < @floor THEN @floor ELSE @allocated + 1 END;
                UPDATE TaxHub.CompaniesHouseAuthoritySequence
                   SET LastAllocated = @allocated, UpdatedAtUtc = SYSUTCDATETIME()
                 WHERE AuthorityEnvironment = N'OfficialTest';
            END;
            SELECT @allocated;
            """, connection, transaction);
        command.Parameters.AddWithValue("@floor", host.OfficialTestSubmissionSequenceFloor);
        var sequence = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        await transaction.CommitAsync(cancellationToken);
        if (sequence is < 1 or > 99999)
            throw new InvalidOperationException("The Companies House official-test sequence is exhausted.");
        return new($"S{sequence:00000}", $"{timeProvider.GetUtcNow():yyyyMMdd}{sequence:00000000}");
    }
}

internal sealed class CompaniesHouseOfficialTestSubmissionMaterializer : ICompaniesHouseSubmissionMaterializer
{
    private readonly CompaniesHouseEnvelopeMaterializer _materializer = new(
        new EnvironmentSecretProvider(TestEnvironmentVariables()));

    public async Task<CompaniesHouseMaterializedSubmission> MaterializeAsync(
        CompaniesHouseAuthorityIdentifiers identifiers,
        CompaniesHouseProtectedContent packagePreview,
        CompaniesHouseProtectedContent accountsDocument,
        CancellationToken cancellationToken = default)
    {
        var result = await _materializer.MaterializeRetainedTestAsync(
            packagePreview.Bytes, packagePreview.Sha256,
            accountsDocument.Bytes, accountsDocument.Sha256,
            identifiers.SubmissionNumber, identifiers.TransactionId, cancellationToken);
        return new(result.Content, result.WireSha256);
    }

    private static IReadOnlyDictionary<SecretReference, string> TestEnvironmentVariables() =>
        new Dictionary<SecretReference, string>
        {
            [CompaniesHouseSecretReferences.TestPresenterId] =
                CompaniesHouseSecretReferences.AppServiceEnvironmentVariables[CompaniesHouseSecretReferences.TestPresenterId],
            [CompaniesHouseSecretReferences.TestPresenterAuthentication] =
                CompaniesHouseSecretReferences.AppServiceEnvironmentVariables[CompaniesHouseSecretReferences.TestPresenterAuthentication],
            [CompaniesHouseSecretReferences.TestFlag] =
                CompaniesHouseSecretReferences.AppServiceEnvironmentVariables[CompaniesHouseSecretReferences.TestFlag],
            [CompaniesHouseSecretReferences.TestPackageReference] =
                CompaniesHouseSecretReferences.AppServiceEnvironmentVariables[CompaniesHouseSecretReferences.TestPackageReference],
            [CompaniesHouseSecretReferences.CompanyAuthentication] =
                CompaniesHouseSecretReferences.AppServiceEnvironmentVariables[CompaniesHouseSecretReferences.CompanyAuthentication]
        };
}

internal sealed class CompaniesHouseOfficialTestSubmissionExchange : ICompaniesHouseSubmissionExchange
{
    private readonly ICompaniesHouseXmlGatewaySession _gateway =
        CompaniesHouseOfficialTestGatewayFactory.Create();

    public Task<CompaniesHouseGatewayResponse> ExchangeAsync(CompaniesHouseMaterializedSubmission request,
        CancellationToken cancellationToken = default) =>
        _gateway.ExchangeAsync(new(CompaniesHouseGatewayOperation.FormSubmission, request.Content),
            cancellationToken);
}

internal sealed class CompaniesHouseOfficialTestStatusRequestMaterializer
    : ICompaniesHouseStatusRequestMaterializer
{
    private readonly CompaniesHouseEnvelopeMaterializer _materializer = new(
        new EnvironmentSecretProvider(TestEnvironmentVariables()));

    public async Task<CompaniesHouseMaterializedSubmission> MaterializeAsync(string submissionNumber,
        string transactionId, CancellationToken cancellationToken = default)
    {
        var result = await _materializer.MaterializeStatusRequestTestAsync(
            TradeControl.Tax.UK.CompaniesHouse.Accounts.Tis6_0.CompaniesHouseStatusQueryScope.Submission,
            transactionId, submissionNumber, cancellationToken: cancellationToken);
        return new(result.Content, result.WireSha256);
    }

    private static IReadOnlyDictionary<SecretReference, string> TestEnvironmentVariables() =>
        new Dictionary<SecretReference, string>
        {
            [CompaniesHouseSecretReferences.TestPresenterId] =
                CompaniesHouseSecretReferences.AppServiceEnvironmentVariables[CompaniesHouseSecretReferences.TestPresenterId],
            [CompaniesHouseSecretReferences.TestPresenterAuthentication] =
                CompaniesHouseSecretReferences.AppServiceEnvironmentVariables[CompaniesHouseSecretReferences.TestPresenterAuthentication],
            [CompaniesHouseSecretReferences.TestFlag] =
                CompaniesHouseSecretReferences.AppServiceEnvironmentVariables[CompaniesHouseSecretReferences.TestFlag]
        };
}

internal sealed class CompaniesHouseOfficialTestStatusExchange : ICompaniesHouseStatusExchange
{
    private readonly ICompaniesHouseXmlGatewaySession _gateway =
        CompaniesHouseOfficialTestGatewayFactory.Create();

    public Task<CompaniesHouseGatewayResponse> ExchangeAsync(CompaniesHouseMaterializedSubmission request,
        CancellationToken cancellationToken = default) =>
        _gateway.ExchangeAsync(new(CompaniesHouseGatewayOperation.GetSubmissionStatus, request.Content),
            cancellationToken);
}
