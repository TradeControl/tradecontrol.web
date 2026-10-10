using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TradeControl.Tax.UK.Application.DataProvision;
using TradeControl.Web.Models;

namespace TradeControl.Web.Data
{
    public sealed record SubjectActionResult(bool Succeeded, string Message)
    {
        public string? SelectedSubjectCode { get; init; }

        public static SubjectActionResult Success(string message = "", string? selectedSubjectCode = null)
            => new(true, message) { SelectedSubjectCode = selectedSubjectCode };

        public static SubjectActionResult Pending(string actionName)
            => new(false, $"{actionName} is not implemented yet.");

        public static SubjectActionResult Failure(string message)
            => new(false, message);
    }

    public sealed record SubjectRemovalPlan
    {
        public NodeEnum.ActionCode ActionCode { get; init; } = NodeEnum.ActionCode.Blocked;
        public bool CanProceed { get; init; }
        public string Message { get; init; } = string.Empty;
        public bool HasOtherParents { get; init; }
        public int AffectedSubjectCount { get; init; }
        public int InvoiceCount { get; init; }
        public int PaymentCount { get; init; }
        public int ProjectCount { get; init; }

        public int TransactionCount => InvoiceCount + PaymentCount + ProjectCount;
        public bool DeletesDetachedClosure => ActionCode == NodeEnum.ActionCode.DeleteDetachedClosure;
        public bool RemovesRelationshipOnly => ActionCode == NodeEnum.ActionCode.RemoveRelationshipOnly;
    }

    public sealed record SubjectReparentPlan
    {
        public NodeEnum.ActionCode ActionCode { get; init; } = NodeEnum.ActionCode.Blocked;
        public bool CanProceed { get; init; }
        public string Message { get; init; } = string.Empty;
        public string OldParentSubjectCode { get; init; } = string.Empty;
        public string NewParentSubjectCode { get; init; } = string.Empty;
        public string ChildSubjectCode { get; init; } = string.Empty;
    }

    public sealed record SubjectAddParentPlan
    {
        public NodeEnum.ActionCode ActionCode { get; init; } = NodeEnum.ActionCode.Blocked;
        public bool CanProceed { get; init; }
        public string Message { get; init; } = string.Empty;
        public string ParentSubjectCode { get; init; } = string.Empty;
        public string ChildSubjectCode { get; init; } = string.Empty;
    }

    public class Subjects
    {
        readonly NodeContext _context;

        public string SubjectCode { get; } = string.Empty;

        public Subjects(NodeContext context)
        {
            _context = context;
        }

        public Subjects(NodeContext context, string accountCode)
        {
            _context = context;
            SubjectCode = accountCode;
        }

        #region Properties
        public async Task<Subject_tbSubject> GetAsync()
        {
            try
            {
                EnsureSubjectCode();
                return await _context.Subject_tbSubjects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return null;
            }
        }

        public async Task<string> AddressCodeAsync()
        {
            try
            {
                EnsureSubjectCode();
                return await _context.Subject_tbSubjects
                    .Where(o => o.SubjectCode == SubjectCode)
                    .Select(o => o.AddressCode)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return string.Empty;
            }
        }

        public Task<string> AddressCode() => AddressCodeAsync();

        public async Task<decimal> BalanceOutstandingAsync(string? parentSubjectCode = null)
        {
            EnsureSubjectCode();
            return await _context.Subject_BalanceOutstanding(SubjectCode, parentSubjectCode);
        }

        public Task<decimal> BalanceOutstanding(string? parentSubjectCode = null) => BalanceOutstandingAsync(parentSubjectCode);

        public async Task<decimal> BalanceToPayAsync()
        {
            EnsureSubjectCode();
            return await _context.BalanceToPay(SubjectCode);
        }

        public Task<decimal> BalanceToPay() => BalanceToPayAsync();

        public async Task<bool> IsNamespaceDefaultAsync(string parentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(parentSubjectCode))
                    return false;

                return await _context.Subject_tbNamespaces
                    .AsNoTracking()
                    .AnyAsync(o => o.ParentSubjectCode == parentSubjectCode
                        && o.ChildSubjectCode == SubjectCode
                        && o.IsDefault);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }
        #endregion

        #region Header Methods

        public async Task<string> DefaultSubjectCodeAsync(string accountName)
        {
            return await _context.SubjectSubjectCodeDefault(accountName);
        }

        public Task<string> DefaultSubjectCode(string accountName) => DefaultSubjectCodeAsync(accountName);

        private void EnsureSubjectCode()
        {
            if (string.IsNullOrWhiteSpace(SubjectCode))
                throw new InvalidOperationException("SubjectCode is required for this operation.");
        }

        private static Task<SubjectActionResult> SubjectNameRequiredAsync()
        {
            return Task.FromResult(SubjectActionResult.Failure("A name is required."));
        }

        public async Task<string> DefaultTaxCodeAsync()
        {
            EnsureSubjectCode();
            return await _context.SubjectTaxCodeDefault(SubjectCode);
        }

        public Task<string> DefaultTaxCode() => DefaultTaxCodeAsync();

        public async Task<string> DefaultEmailAddressAsync()
        {
            EnsureSubjectCode();
            return await _context.SubjectEmailAddressDefault(SubjectCode);
        }

        public Task<string> DefaultEmailAddress() => DefaultEmailAddressAsync();
        public async Task<bool> Rebuild()
        {
            try
            {
                EnsureSubjectCode();
                return await _context.SubjectRebuild(SubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }
        #endregion

        #region Address Methods
        public async Task AddAddressAsync(string address, short addressTypeCode = 0)
        {
            try
            {
                EnsureSubjectCode();
                await _context.Database.ExecuteSqlRawAsync("Subject.proc_AddAddress @p0, @p1, @p2", parameters: [SubjectCode, address, addressTypeCode]);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
            }
        }

        public Task AddAddress(string address) => AddAddressAsync(address);

        public async Task<string> NextAddressCodeAsync()
        {
            EnsureSubjectCode();
            return await _context.NextAddressCode(SubjectCode);
        }

        public Task<string> NextAddressCode() => NextAddressCodeAsync();

        public async Task<Subject_tbAddress> GetAddressAsync(string addressCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(addressCode))
                    return null;

                return await _context.Subject_tbAddresses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode && o.AddressCode == addressCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return null;
            }
        }

        public async Task<SubjectActionResult> UpdateAddressAsync(string addressCode, string address, short addressTypeCode = 0)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(addressCode))
                    return SubjectActionResult.Failure("An address code is required.");

                if (string.IsNullOrWhiteSpace(address))
                    return SubjectActionResult.Failure("Address is required.");

                var current = await _context.Subject_tbAddresses
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode && o.AddressCode == addressCode);

                if (current is null)
                    return SubjectActionResult.Failure("The selected address was not found.");

                current.Address = address.Trim();
                current.AddressTypeCode = addressTypeCode;
                await _context.SaveChangesAsync();

                return SubjectActionResult.Success("Address updated.");
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to update the address.");
            }
        }

        public async Task<SubjectActionResult> DeleteAddressAsync(string addressCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(addressCode))
                    return SubjectActionResult.Failure("An address code is required.");

                var subject = await _context.Subject_tbSubjects
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

                if (subject is null)
                    return SubjectActionResult.Failure("The selected Subject was not found.");

                var address = await _context.Subject_tbAddresses
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode && o.AddressCode == addressCode);

                if (address is null)
                    return SubjectActionResult.Failure("The selected address was not found.");

                var isReferencedByProject = await _context.Project_tbProjects
                    .AsNoTracking()
                    .AnyAsync(project =>
                        project.AddressCodeFrom == addressCode
                        || project.AddressCodeTo == addressCode);

                if (isReferencedByProject)
                    return SubjectActionResult.Failure("This address is referenced by one or more projects and cannot be deleted.");

                var isDefault = string.Equals(subject.AddressCode, addressCode, StringComparison.OrdinalIgnoreCase);
                if (isDefault)
                {
                    subject.AddressCode = await _context.Subject_tbAddresses
                        .Where(o => o.SubjectCode == SubjectCode && o.AddressCode != addressCode)
                        .OrderBy(o => o.AddressCode)
                        .Select(o => o.AddressCode)
                        .FirstOrDefaultAsync();
                }

                _context.Subject_tbAddresses.Remove(address);
                await _context.SaveChangesAsync();

                return SubjectActionResult.Success("Address deleted.");
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to delete the address.");
            }
        }

        public async Task<SubjectActionResult> SetDefaultAddressAsync(string addressCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(addressCode))
                    return SubjectActionResult.Failure("An address code is required.");

                var addressExists = await _context.Subject_tbAddresses
                    .AnyAsync(o => o.SubjectCode == SubjectCode && o.AddressCode == addressCode);

                if (!addressExists)
                    return SubjectActionResult.Failure("The selected address does not belong to this Subject.");

                var subject = await _context.Subject_tbSubjects
                    .FirstOrDefaultAsync(o => o.SubjectCode == SubjectCode);

                if (subject is null)
                    return SubjectActionResult.Failure("The selected Subject was not found.");

                if (string.Equals(subject.AddressCode, addressCode, StringComparison.OrdinalIgnoreCase))
                    return SubjectActionResult.Success("This address is already the default.");

                subject.AddressCode = addressCode;
                await _context.SaveChangesAsync();

                return SubjectActionResult.Success("Default address updated.");
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to update the default address.");
            }
        }
        #endregion

        #region Browser Action Stubs

        public Task<SubjectActionResult> AddStructuralChildAsync(string parentSubjectCode)
            => SubjectNameRequiredAsync();

        public Task<SubjectActionResult> AddStructuralChildAsync(string parentSubjectCode, string subjectName)
            => AddChildAsync(parentSubjectCode, subjectName, NodeEnum.SubjectClass.Structural, "Structural subject");

        public Task<SubjectActionResult> AddRealChildAsync(string parentSubjectCode)
            => SubjectNameRequiredAsync();

        public Task<SubjectActionResult> AddRealChildAsync(string parentSubjectCode, string subjectName)
            => AddChildAsync(parentSubjectCode, subjectName, NodeEnum.SubjectClass.Real, "Person");

        public Task<SubjectActionResult> AddVirtualChildAsync(string parentSubjectCode)
            => SubjectNameRequiredAsync();

        public Task<SubjectActionResult> AddVirtualChildAsync(string parentSubjectCode, string subjectName)
            => AddChildAsync(parentSubjectCode, subjectName, NodeEnum.SubjectClass.Virtual, "Organisation");

        private async Task<SubjectActionResult> AddChildAsync(
            string parentSubjectCode,
            string subjectName,
            NodeEnum.SubjectClass subjectClass,
            string entityLabel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentSubjectCode))
                    return SubjectActionResult.Failure("A parent Subject is required.");

                var normalizedName = subjectName?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedName))
                    return SubjectActionResult.Failure("A name is required.");

                var subjectTypeCode = await ResolveDefaultSubjectTypeCodeAsync(subjectClass);
                if (subjectTypeCode is null)
                    return SubjectActionResult.Failure($"No default Subject type is configured for {entityLabel.ToLowerInvariant()} creation.");

                var outputParameter = new SqlParameter("@SubjectCode", SqlDbType.NVarChar, 50) {
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC Subject.proc_AddNamespace @RootSubjectCode, @SubjectName, @SubjectTypeCode, @SubjectCode OUTPUT",
                    new SqlParameter("@RootSubjectCode", parentSubjectCode),
                    new SqlParameter("@SubjectName", normalizedName),
                    new SqlParameter("@SubjectTypeCode", subjectTypeCode.Value),
                    outputParameter);

                var createdSubjectCode = outputParameter.Value?.ToString();
                if (string.IsNullOrWhiteSpace(createdSubjectCode))
                    return SubjectActionResult.Failure($"Unable to create {entityLabel.ToLowerInvariant()}.");

                return SubjectActionResult.Success($"{entityLabel} created.", createdSubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure($"Unable to create the {entityLabel.ToLowerInvariant()}.");
            }
        }

        private async Task<short?> ResolveDefaultSubjectTypeCodeAsync(NodeEnum.SubjectClass subjectClass)
        {
            return await _context.Subject_tbTypes
                .AsNoTracking()
                .Where(o => o.SubjectClassCode == (short)subjectClass)
                .OrderBy(o => o.SubjectTypeCode)
                .Select(o => (short?)o.SubjectTypeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<SubjectReparentPlan> PreviewReparentAsync(string currentParentSubjectCode, string newParentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(currentParentSubjectCode))
                {
                    return new SubjectReparentPlan {
                        ActionCode = NodeEnum.ActionCode.Blocked,
                        CanProceed = false,
                        Message = "A current parent Subject is required."
                    };
                }

                if (string.IsNullOrWhiteSpace(newParentSubjectCode))
                {
                    return new SubjectReparentPlan {
                        ActionCode = NodeEnum.ActionCode.Blocked,
                        CanProceed = false,
                        Message = "A target parent Subject is required."
                    };
                }

                return await _context.SubjectReparentPreview(currentParentSubjectCode, SubjectCode, newParentSubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return new SubjectReparentPlan {
                    ActionCode = NodeEnum.ActionCode.Blocked,
                    CanProceed = false,
                    Message = "Unable to evaluate move."
                };
            }
        }

        public async Task<SubjectActionResult> ReparentAsync(string currentParentSubjectCode, string newParentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(currentParentSubjectCode))
                    return SubjectActionResult.Failure("A current parent Subject is required.");

                if (string.IsNullOrWhiteSpace(newParentSubjectCode))
                    return SubjectActionResult.Failure("A target parent Subject is required.");

                var plan = await _context.SubjectReparent(currentParentSubjectCode, SubjectCode, newParentSubjectCode);

                return plan.CanProceed
                    ? SubjectActionResult.Success(plan.Message)
                    : SubjectActionResult.Failure(plan.Message);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to move the namespace relationship.");
            }
        }

        public async Task<SubjectRemovalPlan> PreviewRemoveFromNamespaceAsync(string parentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();

                return await _context.SubjectRemoveNamespacePreview(parentSubjectCode ?? string.Empty, SubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return new SubjectRemovalPlan {
                    ActionCode = NodeEnum.ActionCode.Blocked,
                    CanProceed = false,
                    Message = "Unable to evaluate namespace removal."
                };
            }
        }

        public async Task<SubjectActionResult> RemoveFromNamespaceAsync(string parentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();

                var plan = await _context.SubjectRemoveNamespace(parentSubjectCode ?? string.Empty, SubjectCode);

                return plan.CanProceed
                    ? SubjectActionResult.Success(plan.Message)
                    : SubjectActionResult.Failure(plan.Message);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to remove the namespace relationship.");
            }
        }

        public async Task<SubjectActionResult> SetDefaultNamespaceChildAsync(string parentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(parentSubjectCode))
                    return SubjectActionResult.Failure("A parent Subject is required.");

                var namespaceRow = await _context.Subject_tbNamespaces
                    .FirstOrDefaultAsync(o => o.ParentSubjectCode == parentSubjectCode
                        && o.ChildSubjectCode == SubjectCode);

                if (namespaceRow is null)
                    return SubjectActionResult.Failure("The selected namespace relationship was not found.");

                if (namespaceRow.IsDefault)
                    return SubjectActionResult.Success("This Subject is already the default for the selected namespace.");

                namespaceRow.IsDefault = true;
                await _context.SaveChangesAsync();

                return SubjectActionResult.Success("Default namespace child updated.");
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to set the default namespace child.");
            }
        }

        public async Task<SubjectAddParentPlan> PreviewAddToNamespaceAsync(string parentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(parentSubjectCode))
                {
                    return new SubjectAddParentPlan {
                        ActionCode = NodeEnum.ActionCode.Blocked,
                        CanProceed = false,
                        Message = "A parent Subject is required."
                    };
                }

                return await _context.SubjectAddParentPreview(parentSubjectCode, SubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return new SubjectAddParentPlan {
                    ActionCode = NodeEnum.ActionCode.Blocked,
                    CanProceed = false,
                    Message = "Unable to evaluate namespace addition."
                };
            }
        }

        public async Task<SubjectActionResult> AddToNamespaceAsync(string parentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();

                if (string.IsNullOrWhiteSpace(parentSubjectCode))
                    return SubjectActionResult.Failure("A parent Subject is required.");

                var plan = await _context.SubjectAddParent(parentSubjectCode, SubjectCode);

                return plan.CanProceed
                    ? SubjectActionResult.Success(plan.Message)
                    : SubjectActionResult.Failure(plan.Message);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to add the namespace relationship.");
            }
        }

        public async Task<SubjectActionResult> DeleteAsync(string parentSubjectCode)
        {
            try
            {
                EnsureSubjectCode();
                return await RemoveFromNamespaceAsync(parentSubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to delete the selected Subject.");
            }
        }

        public Task<SubjectActionResult> DeleteAsync()
            => DeleteAsync(string.Empty);
        #endregion

        #region Balance Reports

        private const int MaximumBalancePageSize = 100;
        private const int MaximumAgedInvoiceItems = 500;

        public async Task<SubjectCurrentAgedInvoicePage> CurrentAgedInvoicesAsync(
            DateOnly agedOn,
            SubjectBalancePosition position,
            int pageNumber = 1,
            int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, MaximumBalancePageSize);
            var query = _context
                .SubjectCurrentAgedInvoices(agedOn.ToDateTime(TimeOnly.MinValue))
                .AsNoTracking()
                .Where(row => row.PositionCode == (short)position);

            var aggregate = await query
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    TotalCount = group.Count(),
                    HumanBalance = group.Sum(row => Math.Abs(row.BusinessBalance)),
                    CurrentAmount = group.Sum(row => row.CurrentAmount),
                    Days1To30Amount = group.Sum(row => row.Days1To30Amount),
                    Days31To60Amount = group.Sum(row => row.Days31To60Amount),
                    Days61To90Amount = group.Sum(row => row.Days61To90Amount),
                    Over90Amount = group.Sum(row => row.Over90Amount),
                    StatementEquivalent = group.Sum(row => row.StatementBusinessBalance * (row.PositionCode == 0 ? 1 : -1)),
                    ReconciliationResidual = group.Sum(row => row.ReconciliationResidual * (row.PositionCode == 0 ? 1 : -1))
                })
                .SingleOrDefaultAsync(cancellationToken);

            var items = await query
                .OrderByDescending(row => Math.Abs(row.BusinessBalance))
                .ThenBy(row => row.SubjectName)
                .ThenBy(row => row.SubjectCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(row => new SubjectCurrentAgedInvoiceRow(
                    row.SubjectCode,
                    row.SubjectName,
                    DateOnly.FromDateTime(row.AgedOn),
                    row.BusinessBalance,
                    (SubjectBalancePosition)row.PositionCode,
                    row.CurrentAmount,
                    row.Days1To30Amount,
                    row.Days31To60Amount,
                    row.Days61To90Amount,
                    row.Over90Amount,
                    row.StatementBusinessBalance,
                    row.ReconciliationResidual))
                .ToListAsync(cancellationToken);

            var totals = aggregate is null
                ? new SubjectCurrentAgedInvoiceTotals(0, 0, 0, 0, 0, 0, 0, 0)
                : new SubjectCurrentAgedInvoiceTotals(
                    aggregate.HumanBalance,
                    aggregate.CurrentAmount,
                    aggregate.Days1To30Amount,
                    aggregate.Days31To60Amount,
                    aggregate.Days61To90Amount,
                    aggregate.Over90Amount,
                    aggregate.StatementEquivalent,
                    aggregate.ReconciliationResidual);

            return new SubjectCurrentAgedInvoicePage
            {
                AgedOn = agedOn,
                Position = position,
                Items = items,
                Totals = totals,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = aggregate?.TotalCount ?? 0
            };
        }

        public async Task<SubjectCurrentAgedInvoiceDetail?> CurrentAgedInvoiceDetailAsync(
            DateOnly agedOn,
            string? subjectCode = null,
            int maxItems = 250,
            CancellationToken cancellationToken = default)
        {
            var resolvedSubjectCode = string.IsNullOrWhiteSpace(subjectCode) ? SubjectCode : subjectCode.Trim();
            if (string.IsNullOrWhiteSpace(resolvedSubjectCode))
                throw new ArgumentException("A Subject code is required.", nameof(subjectCode));

            maxItems = Math.Clamp(maxItems, 1, MaximumAgedInvoiceItems);

            var agedOnDateTime = agedOn.ToDateTime(TimeOnly.MinValue);
            var summaryData = await _context
                .SubjectCurrentAgedInvoices(agedOnDateTime)
                .AsNoTracking()
                .SingleOrDefaultAsync(row => row.SubjectCode == resolvedSubjectCode, cancellationToken);

            if (summaryData is null)
                return null;

            var itemQuery = _context
                .SubjectCurrentAgedInvoiceItems(agedOnDateTime)
                .AsNoTracking()
                .Where(row => row.SubjectCode == resolvedSubjectCode);

            var totalItemCount = await itemQuery.CountAsync(cancellationToken);
            var items = await itemQuery
                .OrderBy(row => row.DueOn)
                .ThenBy(row => row.InvoiceNumber)
                .Take(maxItems)
                .Select(row => new SubjectCurrentAgedInvoiceItem(
                    row.InvoiceNumber,
                    row.InvoiceTypeCode,
                    row.InvoiceType,
                    DateOnly.FromDateTime(row.InvoicedOn),
                    DateOnly.FromDateTime(row.DueOn),
                    row.DaysOverdue,
                    (SubjectAgeBand)row.AgeBandCode,
                    row.BusinessAmount))
                .ToListAsync(cancellationToken);

            var summary = new SubjectCurrentAgedInvoiceRow(
                summaryData.SubjectCode,
                summaryData.SubjectName,
                DateOnly.FromDateTime(summaryData.AgedOn),
                summaryData.BusinessBalance,
                (SubjectBalancePosition)summaryData.PositionCode,
                summaryData.CurrentAmount,
                summaryData.Days1To30Amount,
                summaryData.Days31To60Amount,
                summaryData.Days61To90Amount,
                summaryData.Over90Amount,
                summaryData.StatementBusinessBalance,
                summaryData.ReconciliationResidual);

            return new SubjectCurrentAgedInvoiceDetail
            {
                Summary = summary,
                Items = items,
                TotalItemCount = totalItemCount
            };
        }

        public async Task<SubjectDatedBalancePage> DatedBalancesAsync(
            DateOnly asOfDate,
            SubjectBalancePosition position,
            int pageNumber = 1,
            int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, MaximumBalancePageSize);

            var query = _context
                .SubjectDatedBalances(asOfDate.ToDateTime(TimeOnly.MinValue))
                .AsNoTracking()
                .Where(row => row.PositionCode == (short)position);

            var aggregate = await query
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    TotalCount = group.Count(),
                    TotalHumanBalance = group.Sum(row => row.HumanBalance)
                })
                .SingleOrDefaultAsync(cancellationToken);

            var items = await query
                .OrderByDescending(row => row.HumanBalance)
                .ThenBy(row => row.SubjectName)
                .ThenBy(row => row.SubjectCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(row => new SubjectDatedBalanceRow(
                    row.SubjectCode,
                    row.SubjectName,
                    DateOnly.FromDateTime(row.AsOfDate),
                    row.NativeStatementBalance,
                    row.BusinessBalance,
                    (SubjectBalancePosition)row.PositionCode,
                    row.HumanBalance))
                .ToListAsync(cancellationToken);

            return new SubjectDatedBalancePage
            {
                AsOfDate = asOfDate,
                Position = position,
                Items = items,
                TotalHumanBalance = aggregate?.TotalHumanBalance ?? 0,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = aggregate?.TotalCount ?? 0
            };
        }

        public async Task<SubjectBalanceEvidenceSnapshot> YearEndBalanceSnapshotAsync(
            DateOnly effectiveDate,
            CancellationToken cancellationToken = default)
        {
            var query = _context
                .SubjectDatedBalances(effectiveDate.ToDateTime(TimeOnly.MinValue))
                .AsNoTracking();

            var source = await query
                .GroupBy(_ => 1)
                .Select(group => new SubjectBalanceEvidenceSourceSummary(
                    group.Count(row => row.PositionCode == (short)SubjectBalancePosition.OwedToUs),
                    group.Count(row => row.PositionCode == (short)SubjectBalancePosition.OwedByUs),
                    group.Where(row => row.PositionCode == (short)SubjectBalancePosition.OwedToUs)
                        .Sum(row => row.HumanBalance),
                    group.Where(row => row.PositionCode == (short)SubjectBalancePosition.OwedByUs)
                        .Sum(row => row.HumanBalance)))
                .SingleOrDefaultAsync(cancellationToken)
                ?? new SubjectBalanceEvidenceSourceSummary(0, 0, 0m, 0m);

            var items = await query
                .OrderBy(row => row.SubjectCode)
                .Select(row => new SubjectBalanceEvidenceItem(
                    row.SubjectCode,
                    row.SubjectName,
                    row.NativeStatementBalance,
                    row.BusinessBalance,
                    row.PositionCode == (short)SubjectBalancePosition.OwedToUs
                        ? SubjectBalanceEvidencePosition.Debtor
                        : SubjectBalanceEvidencePosition.Creditor))
                .ToListAsync(cancellationToken);

            return SubjectBalanceEvidenceSnapshot.Create(effectiveDate, items, source);
        }

        #endregion

        #region Payment Related Methods

        public Task<SubjectActionResult> AddChildByTypeAsync(string parentSubjectCode)
            => SubjectNameRequiredAsync();

        public async Task<SubjectActionResult> AddChildByTypeAsync(
            string parentSubjectCode,
            string subjectName,
            short subjectTypeCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentSubjectCode))
                    return SubjectActionResult.Failure("A parent Subject is required.");

                var normalizedName = subjectName?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedName))
                    return SubjectActionResult.Failure("A name is required.");

                var subjectType = await _context.Subject_tbTypes
                    .AsNoTracking()
                    .Where(o => o.SubjectTypeCode == subjectTypeCode)
                    .Select(o => new
                    {
                        o.SubjectTypeCode,
                        o.SubjectType,
                        o.SubjectClassCode
                    })
                    .FirstOrDefaultAsync();

                if (subjectType is null)
                    return SubjectActionResult.Failure("The selected Subject type was not found.");

                if (subjectType.SubjectClassCode == (short)NodeEnum.SubjectClass.Structural)
                    return SubjectActionResult.Failure("Structural Subject types are not available from Payments.");

                var outputParameter = new SqlParameter("@SubjectCode", SqlDbType.NVarChar, 50) {
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC Subject.proc_AddNamespace @RootSubjectCode, @SubjectName, @SubjectTypeCode, @SubjectCode OUTPUT",
                    new SqlParameter("@RootSubjectCode", parentSubjectCode),
                    new SqlParameter("@SubjectName", normalizedName),
                    new SqlParameter("@SubjectTypeCode", subjectType.SubjectTypeCode),
                    outputParameter);

                var createdSubjectCode = outputParameter.Value?.ToString();
                if (string.IsNullOrWhiteSpace(createdSubjectCode))
                    return SubjectActionResult.Failure("Unable to create the selected Subject.");

                return SubjectActionResult.Success($"{subjectType.SubjectType} created.", createdSubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure("Unable to create the selected Subject.");
            }
        }
        #endregion

        #region Root Action Stubs

        public Task<SubjectActionResult> AddStructuralRootAsync(string subjectName)
            => AddRootAsync(subjectName, NodeEnum.SubjectClass.Structural, "Structural subject");

        public Task<SubjectActionResult> AddRealRootAsync(string subjectName)
            => AddRootAsync(subjectName, NodeEnum.SubjectClass.Real, "Person");

        public Task<SubjectActionResult> AddVirtualRootAsync(string subjectName)
            => AddRootAsync(subjectName, NodeEnum.SubjectClass.Virtual, "Organisation");

        private async Task<SubjectActionResult> AddRootAsync(
            string subjectName,
            NodeEnum.SubjectClass subjectClass,
            string entityLabel)
        {
            try
            {
                var normalizedName = subjectName?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedName))
                    return SubjectActionResult.Failure("A name is required.");

                var subjectTypeCode = await ResolveDefaultSubjectTypeCodeAsync(subjectClass);
                if (subjectTypeCode is null)
                    return SubjectActionResult.Failure($"No default Subject type is configured for {entityLabel.ToLowerInvariant()} creation.");

                var createdSubjectCode = await _context.SubjectAddRoot(normalizedName, subjectTypeCode.Value);
                if (string.IsNullOrWhiteSpace(createdSubjectCode))
                    return SubjectActionResult.Failure($"Unable to create {entityLabel.ToLowerInvariant()}.");

                return SubjectActionResult.Success($"{entityLabel} created.", createdSubjectCode);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return SubjectActionResult.Failure($"Unable to create the {entityLabel.ToLowerInvariant()}.");
            }
        }
        #endregion

    }
}
