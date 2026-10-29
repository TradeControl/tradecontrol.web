using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.FileProviders;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices;

public sealed record CustomerExitArchive(
    byte[] Bytes,
    string FileName,
    string Sha256,
    string ExportId,
    int TableCount,
    int FileCount);

public sealed record CustomerExitTable(
    string Schema,
    string Name,
    IReadOnlyList<string> Columns);

public interface ICustomerExitDataSource
{
    IReadOnlyList<CustomerExitTable> Tables { get; }

    Task WriteTableAsync(
        CustomerExitTable table,
        Stream destination,
        CancellationToken cancellationToken);
}

public interface ICustomerExitArchiveService
{
    Task<CustomerExitArchive> CreateAsync(string requestedBy, CancellationToken cancellationToken);
}

public interface ICustomerExitAudit
{
    Task RecordAsync(string message);
}

public sealed class CustomerExitAudit(NodeContext nodeContext) : ICustomerExitAudit
{
    public Task RecordAsync(string message) =>
        nodeContext.EventLog(NodeEnum.EventType.IsInformation, message);
}

public sealed class CustomerExitArchiveService(
    ICustomerExitDataSource dataSource,
    IFileProvider tenantFiles,
    ICustomerExitAudit audit,
    TimeProvider timeProvider) : ICustomerExitArchiveService
{
    private static readonly string[] TenantContentFolders =
        ["documents", "images", "templates"];

    public async Task<CustomerExitArchive> CreateAsync(
        string requestedBy,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedBy);

        var createdAt = timeProvider.GetUtcNow();
        var exportId = Guid.NewGuid().ToString("N");
        var fileCount = 0;

        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifest = new
            {
                format = "Trade Control customer exit archive",
                formatVersion = 1,
                exportId,
                createdAtUtc = createdAt,
                dataFormat = "UTF-8 JSON arrays; binary tenant files retain their original format",
                scope = "The current tenant database and current tenant-owned content only",
                exclusions = new[]
                {
                    "Infrastructure configuration and operational event logs",
                    "Host passwords, encryption keys and initialisation vectors",
                    "OAuth grants, access tokens, refresh tokens and vendor credentials",
                    "Central identity credentials and data belonging to any other tenant"
                },
                tables = dataSource.Tables.Select(table => new
                {
                    schema = table.Schema,
                    name = table.Name,
                    columns = table.Columns,
                    path = $"data/{SafeSegment(table.Schema)}.{SafeSegment(table.Name)}.json"
                })
            };

            await WriteJsonEntryAsync(
                archive,
                "manifest.json",
                manifest,
                cancellationToken);

            foreach (var table in dataSource.Tables)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = archive.CreateEntry(
                    $"data/{SafeSegment(table.Schema)}.{SafeSegment(table.Name)}.json",
                    CompressionLevel.Optimal);
                await using var destination = entry.Open();
                await dataSource.WriteTableAsync(table, destination, cancellationToken);
            }

            foreach (var folder in TenantContentFolders)
            {
                var contents = tenantFiles.GetDirectoryContents($"content/{folder}");
                if (!contents.Exists)
                    continue;

                foreach (var file in contents
                    .Where(item => !item.IsDirectory)
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var entry = archive.CreateEntry(
                        $"files/{folder}/{SafeSegment(file.Name)}",
                        CompressionLevel.Optimal);
                    await using var source = file.CreateReadStream();
                    await using var destination = entry.Open();
                    await source.CopyToAsync(destination, cancellationToken);
                    fileCount++;
                }
            }
        }

        var bytes = output.ToArray();
        var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        await audit.RecordAsync(
            $"Customer exit archive {exportId} generated by {requestedBy}; " +
            $"tables={dataSource.Tables.Count}, files={fileCount}, sha256={sha256}.");

        return new CustomerExitArchive(
            bytes,
            $"tradecontrol-customer-exit-{createdAt:yyyyMMdd-HHmmss}.zip",
            sha256,
            exportId,
            dataSource.Tables.Count,
            fileCount);
    }

    private static async Task WriteJsonEntryAsync<T>(
        ZipArchive archive,
        string path,
        T value,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        await using var destination = entry.Open();
        await JsonSerializer.SerializeAsync(
            destination,
            value,
            new JsonSerializerOptions { WriteIndented = true },
            cancellationToken);
    }

    private static string SafeSegment(string value) =>
        string.Concat(value.Select(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' or '.'
                ? character
                : '_'));
}

public sealed class EfCustomerExitDataSource : ICustomerExitDataSource
{
    private static readonly HashSet<(string Schema, string Table)> ExcludedTables = new()
    {
        ("App", "tbHost"),
        ("App", "tbEventLog")
    };

    private static readonly string[] SensitiveNameFragments =
        [
            "password",
            "secret",
            "token",
            "credential",
            "connectionstring",
            "privatekey",
            "symmetrickey",
            "symmetriciv",
            "hostid"
        ];

    private readonly NodeContext _nodeContext;

    public EfCustomerExitDataSource(NodeContext nodeContext)
    {
        _nodeContext = nodeContext;
        Tables = BuildTableList(nodeContext.Model);
    }

    public IReadOnlyList<CustomerExitTable> Tables { get; }

    public async Task WriteTableAsync(
        CustomerExitTable table,
        Stream destination,
        CancellationToken cancellationToken)
    {
        var connection = _nodeContext.Database.GetDbConnection();
        var closeConnection = connection.State != ConnectionState.Open;
        if (closeConnection)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"SELECT {string.Join(", ", table.Columns.Select(QuoteIdentifier))} " +
                $"FROM {QuoteIdentifier(table.Schema)}.{QuoteIdentifier(table.Name)}";

            await using var reader = await command.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess,
                cancellationToken);
            await using var writer = new Utf8JsonWriter(destination, new JsonWriterOptions { Indented = true });
            writer.WriteStartArray();
            while (await reader.ReadAsync(cancellationToken))
            {
                writer.WriteStartObject();
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    writer.WritePropertyName(reader.GetName(index));
                    if (await reader.IsDBNullAsync(index, cancellationToken))
                    {
                        writer.WriteNullValue();
                        continue;
                    }

                    WriteValue(writer, reader.GetValue(index));
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            await writer.FlushAsync(cancellationToken);
        }
        finally
        {
            if (closeConnection)
                await connection.CloseAsync();
        }
    }

    private static IReadOnlyList<CustomerExitTable> BuildTableList(IModel model) =>
        model.GetEntityTypes()
            .Where(entity => entity.GetViewName() is null && entity.GetTableName() is not null)
            .GroupBy(entity => (Schema: entity.GetSchema() ?? "dbo", Table: entity.GetTableName()!))
            .Where(group => !ExcludedTables.Contains(group.Key))
            .Select(group =>
            {
                var identifier = StoreObjectIdentifier.Table(group.Key.Table, group.Key.Schema);
                var columns = group
                    .SelectMany(entity => entity.GetProperties())
                    .Where(property => !property.IsShadowProperty())
                    .Select(property => property.GetColumnName(identifier))
                    .Where(column => !string.IsNullOrWhiteSpace(column))
                    .Select(column => column!)
                    .Where(column => !SensitiveNameFragments.Any(fragment =>
                        column.Replace("_", string.Empty, StringComparison.Ordinal)
                            .Contains(fragment, StringComparison.OrdinalIgnoreCase)))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(column => column, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                return new CustomerExitTable(group.Key.Schema, group.Key.Table, columns);
            })
            .Where(table => table.Columns.Count > 0)
            .OrderBy(table => table.Schema, StringComparer.OrdinalIgnoreCase)
            .ThenBy(table => table.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string QuoteIdentifier(string value) =>
        $"[{value.Replace("]", "]]", StringComparison.Ordinal)}]";

    private static void WriteValue(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case bool boolean: writer.WriteBooleanValue(boolean); break;
            case byte number: writer.WriteNumberValue(number); break;
            case short number: writer.WriteNumberValue(number); break;
            case int number: writer.WriteNumberValue(number); break;
            case long number: writer.WriteNumberValue(number); break;
            case float number: writer.WriteNumberValue(number); break;
            case double number: writer.WriteNumberValue(number); break;
            case decimal number: writer.WriteNumberValue(number); break;
            case Guid guid: writer.WriteStringValue(guid); break;
            case DateTime dateTime: writer.WriteStringValue(dateTime); break;
            case DateTimeOffset dateTimeOffset: writer.WriteStringValue(dateTimeOffset); break;
            case TimeSpan timeSpan: writer.WriteStringValue(timeSpan.ToString("c", System.Globalization.CultureInfo.InvariantCulture)); break;
            case byte[] bytes: writer.WriteBase64StringValue(bytes); break;
            default: writer.WriteStringValue(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)); break;
        }
    }
}
