using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        public async Task AddAddressAsync(string address)
        {
            try
            {
                EnsureSubjectCode();
                await _context.Database.ExecuteSqlRawAsync("Subject.proc_AddAddress @p0, @p1", parameters: [SubjectCode, address]);
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

        public async Task<SubjectActionResult> UpdateAddressAsync(string addressCode, string address)
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
