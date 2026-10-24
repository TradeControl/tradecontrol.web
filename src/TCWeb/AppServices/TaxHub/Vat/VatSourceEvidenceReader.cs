using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TradeControl.Tax.UK.Application.StatutoryEvidence;

namespace TradeControl.Web.AppServices.TaxHub.Vat;

internal sealed record VatSourceEvidenceSnapshot(
    string DatabaseInstanceReference,
    string ReportingSubjectReference,
    long AccountingEventId,
    string SqlNodeVersion,
    string SourceInventorySha256,
    IReadOnlyList<VatSealAdjustment> Adjustments)
{
    public bool SameBoundaryAndContent(VatSourceEvidenceSnapshot other) =>
        DatabaseInstanceReference == other.DatabaseInstanceReference
        && ReportingSubjectReference == other.ReportingSubjectReference
        && SqlNodeVersion == other.SqlNodeVersion
        && SourceInventorySha256 == other.SourceInventorySha256
        && Adjustments.SequenceEqual(other.Adjustments);
}

internal sealed class VatSourceEvidenceReader(string connectionString)
{
    public async Task<VatSourceEvidenceSnapshot> ReadAsync(DateOnly periodStart, DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("App.proc_VatSourceEvidence", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@PeriodStart", SqlDbType.Date).Value = periodStart.ToDateTime(TimeOnly.MinValue);
        command.Parameters.Add("@PeriodEnd", SqlDbType.Date).Value = periodEnd.ToDateTime(TimeOnly.MinValue);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("The database statutory identity is unavailable.");
        var database = Required(reader, "DatabaseInstanceReference");
        var subject = Required(reader, "ReportingSubjectReference");
        var eventId = Convert.ToInt64(reader["AccountingEventId"]);
        var sqlNodeVersion = Required(reader, "SqlNodeVersion");
        if (await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("The database statutory identity is ambiguous.");

        if (!await reader.NextResultAsync(cancellationToken))
            throw new InvalidOperationException("The VAT adjustment evidence is unavailable.");
        var adjustments = new List<VatSealAdjustment>();
        while (await reader.ReadAsync(cancellationToken))
            adjustments.Add(new(DateOnly.FromDateTime(Convert.ToDateTime(reader["PeriodStart"])),
                Convert.ToDecimal(reader["Amount"]),
                reader["Explanation"] == DBNull.Value ? null : Convert.ToString(reader["Explanation"])));

        if (!await reader.NextResultAsync(cancellationToken))
            throw new InvalidOperationException("The VAT source inventory is unavailable.");
        var inventory = new List<StatutorySourceInventoryItem>();
        while (await reader.ReadAsync(cancellationToken))
            inventory.Add(new(Required(reader, "SourceObject"), Required(reader, "CanonicalState")));
        return new(database, subject, eventId, sqlNodeVersion,
            StatutorySourceInventory.Sha256(inventory), adjustments);
    }

    private static string Required(SqlDataReader reader, string column)
    {
        var value = reader[column] == DBNull.Value ? null : Convert.ToString(reader[column]);
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"VAT evidence column {column} is unavailable.")
            : value;
    }
}
