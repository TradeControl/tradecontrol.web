using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Tax.UK.Application.StatutoryEvidence;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices.TaxHub;

public sealed record CategoryTreeSubmissionLock(
    CategoryTreeSealEvidence CategoryTree,
    string WorkflowReference,
    string EvidenceSha256);

public interface ICategoryTreeSubmissionLockService
{
    Task AcquireAsync(CategoryTreeSubmissionLock submissionLock,
        CancellationToken cancellationToken = default);
    Task ReleaseAsync(CategoryTreeSubmissionLock submissionLock,
        CancellationToken cancellationToken = default);
    Task CompleteAsync(CategoryTreeSubmissionLock submissionLock, string outcomeNote,
        CancellationToken cancellationToken = default);
}

internal sealed class CategoryTreeSubmissionLockService(NodeContext nodeContext)
    : ICategoryTreeSubmissionLockService
{
    public Task AcquireAsync(CategoryTreeSubmissionLock submissionLock,
        CancellationToken cancellationToken = default) => ExecuteAsync(
        "Cash.proc_CategoryVersionSubmissionLockAcquire", submissionLock, cancellationToken,
        ("@CanonicalProfile", submissionLock.CategoryTree.CanonicalProfile, DbType.String),
        ("@CanonicalSha256", Digest(submissionLock.CategoryTree.CanonicalSha256), DbType.Binary),
        ("@CanonicalByteLength", submissionLock.CategoryTree.CanonicalByteLength, DbType.Int64));

    public Task ReleaseAsync(CategoryTreeSubmissionLock submissionLock,
        CancellationToken cancellationToken = default) => ExecuteAsync(
        "Cash.proc_CategoryVersionSubmissionLockRelease", submissionLock, cancellationToken);

    public Task CompleteAsync(CategoryTreeSubmissionLock submissionLock, string outcomeNote,
        CancellationToken cancellationToken = default) => ExecuteAsync(
        "Cash.proc_CategoryVersionSubmissionComplete", submissionLock, cancellationToken,
        ("@ChangeNote", outcomeNote, DbType.String));

    private async Task ExecuteAsync(string procedure, CategoryTreeSubmissionLock submissionLock,
        CancellationToken cancellationToken, params (string Name, object Value, DbType Type)[] additional)
    {
        CategoryTreeStatutoryEvidence.Validate(submissionLock.CategoryTree);
        if (string.IsNullOrWhiteSpace(submissionLock.WorkflowReference)
            || submissionLock.WorkflowReference.Length > 64)
            throw new ArgumentException("A durable statutory workflow reference is required.", nameof(submissionLock));

        var connection = nodeContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedure;
            Add(command, "@VersionId", submissionLock.CategoryTree.VersionId, DbType.String);
            Add(command, "@SubmissionLockReference", submissionLock.WorkflowReference, DbType.String);
            Add(command, "@SubmissionLockSha256", Digest(submissionLock.EvidenceSha256), DbType.Binary);
            foreach (var parameter in additional)
                Add(command, parameter.Name, parameter.Value, parameter.Type);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (close) await connection.CloseAsync();
        }
    }

    private static void Add(System.Data.Common.DbCommand command, string name, object value, DbType type)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static byte[] Digest(string value)
    {
        if (value is not { Length: 64 } || !value.All(Uri.IsHexDigit))
            throw new ArgumentException("A SHA-256 digest is required.", nameof(value));
        return Convert.FromHexString(value);
    }
}
