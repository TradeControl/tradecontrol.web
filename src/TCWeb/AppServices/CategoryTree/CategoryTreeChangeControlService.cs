using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TradeControl.Web.Authorization;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using CategoryTreeStatus = TradeControl.Web.Data.NodeEnum.CategoryVersionStatus;

namespace TradeControl.Web.AppServices.CategoryTree;

public sealed class CategoryTreeChangeControlService : ICategoryTreeChangeControlService
{
    private readonly IDbContextFactory<NodeContext> _contextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryTreeChangeControlService(IDbContextFactory<NodeContext> contextFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _contextFactory = contextFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CategoryTreeWorkspaceContext> GetWorkspaceAsync(string? requestedVersionId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var activeVersionId = await context.App_tbOptions
            .AsNoTracking()
            .Select(options => options.CategoryVersionId)
            .SingleAsync(cancellationToken);

        var versions = await context.Cash_tbCategoryVersions
            .AsNoTracking()
            .OrderByDescending(version => version.CreatedOn)
            .ThenByDescending(version => version.VersionId)
            .ToListAsync(cancellationToken);

        var statusNames = await context.Cash_tbCategoryVersionStatuses
            .AsNoTracking()
            .ToDictionaryAsync(status => status.VersionStatusCode, status => status.VersionStatus,
                cancellationToken);

        var validationTimes = await context.Cash_tbCategoryVersionEvents
            .AsNoTracking()
            .Where(versionEvent => versionEvent.EventTypeCode ==
                (short)NodeEnum.CategoryVersionEventType.Validate)
            .GroupBy(versionEvent => versionEvent.VersionId)
            .Select(group => new { VersionId = group.Key, EventOn = group.Max(item => item.EventOn) })
            .ToDictionaryAsync(item => item.VersionId, item => item.EventOn, cancellationToken);

        var byId = versions.ToDictionary(version => version.VersionId, StringComparer.Ordinal);
        if (!byId.TryGetValue(activeVersionId, out _))
        {
            throw new InvalidOperationException(
                $"The active Category Tree version '{activeVersionId}' was not found.");
        }

        string? notice = null;
        var selectedVersionId = string.IsNullOrWhiteSpace(requestedVersionId)
            ? activeVersionId
            : requestedVersionId.Trim();

        if (!byId.ContainsKey(selectedVersionId))
        {
            notice = $"Category Tree version '{selectedVersionId}' is unavailable. The active version is displayed.";
            selectedVersionId = activeVersionId;
        }

        var isAdministrator = _httpContextAccessor.HttpContext?.User
            .IsInRole(Constants.AdministratorsRole) == true;

        var versionInfo = versions
            .Select(version => ToVersionInfo(version, activeVersionId, byId, statusNames,
                validationTimes, isAdministrator))
            .ToArray();

        var selected = versionInfo.Single(version => version.VersionId == selectedVersionId);
        var events = await context.Cash_tbCategoryVersionEvents
            .AsNoTracking()
            .Where(versionEvent => versionEvent.VersionId == selectedVersionId)
            .OrderByDescending(versionEvent => versionEvent.EventOn)
            .ThenByDescending(versionEvent => versionEvent.VersionEventId)
            .Select(versionEvent => new CategoryTreeVersionEventInfo(
                versionEvent.VersionEventId,
                versionEvent.VersionId,
                versionEvent.EventTypeCode,
                ToNullableStatus(versionEvent.FromStatusCode),
                ToNullableStatus(versionEvent.ToStatusCode),
                versionEvent.EventOn,
                versionEvent.UserId,
                versionEvent.UserName,
                versionEvent.ChangeNote,
                ToHex(versionEvent.ContentSha256)))
            .ToListAsync(cancellationToken);

        return new(activeVersionId, selected, versionInfo, events,
            Consequence(selected.Status, selected.IsActive, activeVersionId), notice);
    }

    public async Task<CategoryTreeVersionReview> GetReviewAsync(string versionId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var version = await context.Cash_tbCategoryVersions.AsNoTracking()
            .SingleAsync(item => item.VersionId == versionId, cancellationToken);
        if (version.PredecessorVersionId is null)
        {
            return new(versionId, null, Array.Empty<CategoryTreeChangeGroup>(),
                new[] { "This version has no recorded predecessor." });
        }

        var predecessorId = version.PredecessorVersionId;
        var categoryRows = await context.Cash_tbCategories.AsNoTracking()
            .Where(item => item.VersionId == versionId || item.VersionId == predecessorId)
            .OrderBy(item => item.CategoryCode)
            .ToListAsync(cancellationToken);
        var edgeRows = await context.Cash_tbCategoryTotals.AsNoTracking()
            .Where(item => item.VersionId == versionId || item.VersionId == predecessorId)
            .OrderBy(item => item.ParentCode).ThenBy(item => item.ChildCode)
            .ToListAsync(cancellationToken);
        var mappingRows = await context.Cash_tbCategoryCodeMaps.AsNoTracking()
            .Where(item => item.VersionId == versionId || item.VersionId == predecessorId)
            .OrderBy(item => item.CashCode)
            .ToListAsync(cancellationToken);
        var rootRows = await context.Cash_tbCategoryRoots.AsNoTracking()
            .Where(item => item.VersionId == versionId || item.VersionId == predecessorId)
            .OrderBy(item => item.RootTypeCode)
            .ToListAsync(cancellationToken);

        var groups = new List<CategoryTreeChangeGroup>();
        AddComparisonGroup(groups, "Categories added", categoryRows, versionId, predecessorId,
            item => item.VersionId, item => item.CategoryCode);
        AddComparisonGroup(groups, "Categories removed", categoryRows, predecessorId, versionId,
            item => item.VersionId, item => item.CategoryCode);

        var currentCategories = categoryRows.Where(item => item.VersionId == versionId)
            .ToDictionary(item => item.CategoryCode, StringComparer.Ordinal);
        var predecessorCategories = categoryRows.Where(item => item.VersionId == predecessorId)
            .ToDictionary(item => item.CategoryCode, StringComparer.Ordinal);
        var changedCategories = currentCategories.Keys.Intersect(predecessorCategories.Keys,
                StringComparer.Ordinal)
            .Where(code => !CategoryContentEquals(currentCategories[code], predecessorCategories[code]))
            .OrderBy(code => code, StringComparer.Ordinal).ToArray();
        AddGroup(groups, "Categories changed", changedCategories);

        var currentEdges = edgeRows.Where(item => item.VersionId == versionId)
            .ToDictionary(item => $"{item.ParentCode} → {item.ChildCode}", StringComparer.Ordinal);
        var predecessorEdges = edgeRows.Where(item => item.VersionId == predecessorId)
            .ToDictionary(item => $"{item.ParentCode} → {item.ChildCode}", StringComparer.Ordinal);
        AddGroup(groups, "Links added", currentEdges.Keys.Except(predecessorEdges.Keys,
            StringComparer.Ordinal));
        AddGroup(groups, "Links removed", predecessorEdges.Keys.Except(currentEdges.Keys,
            StringComparer.Ordinal));
        AddGroup(groups, "Links reordered", currentEdges.Keys.Intersect(predecessorEdges.Keys,
                StringComparer.Ordinal)
            .Where(key => currentEdges[key].DisplayOrder != predecessorEdges[key].DisplayOrder));

        var currentMappings = mappingRows.Where(item => item.VersionId == versionId)
            .ToDictionary(item => item.CashCode, item => item.CategoryCode, StringComparer.Ordinal);
        var predecessorMappings = mappingRows.Where(item => item.VersionId == predecessorId)
            .ToDictionary(item => item.CashCode, item => item.CategoryCode, StringComparer.Ordinal);
        AddGroup(groups, "Cash Code mappings added", currentMappings.Keys
            .Except(predecessorMappings.Keys, StringComparer.Ordinal)
            .Select(code => $"{code} → {currentMappings[code]}"));
        AddGroup(groups, "Cash Code mappings removed", predecessorMappings.Keys
            .Except(currentMappings.Keys, StringComparer.Ordinal)
            .Select(code => $"{code} → {predecessorMappings[code]}"));
        AddGroup(groups, "Cash Codes reclassified", currentMappings.Keys
            .Intersect(predecessorMappings.Keys, StringComparer.Ordinal)
            .Where(code => currentMappings[code] != predecessorMappings[code])
            .Select(code => $"{code}: {predecessorMappings[code]} → {currentMappings[code]}"));

        var currentRoots = rootRows.Where(item => item.VersionId == versionId)
            .ToDictionary(item => item.RootTypeCode, item => item.CategoryCode);
        var predecessorRoots = rootRows.Where(item => item.VersionId == predecessorId)
            .ToDictionary(item => item.RootTypeCode, item => item.CategoryCode);
        AddGroup(groups, "Roots changed", currentRoots.Keys.Union(predecessorRoots.Keys)
            .Where(code => currentRoots.GetValueOrDefault(code) != predecessorRoots.GetValueOrDefault(code))
            .Select(code => $"{RootName(code)}: {predecessorRoots.GetValueOrDefault(code) ?? "not set"} → {currentRoots.GetValueOrDefault(code) ?? "not set"}"));

        var warnings = new List<string>();
        if (PredecessorIsStale(version, await context.Cash_tbCategoryVersions.AsNoTracking()
                .ToDictionaryAsync(item => item.VersionId, cancellationToken)))
        {
            warnings.Add("The active predecessor changed after this Draft was cloned.");
        }
        return new(versionId, predecessorId, groups, warnings);
    }

    public Task<CategoryTreeCommandResult> CloneActiveAsync(string expectedActiveVersionId,
        string description, string? notes,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("A successor Draft requires a description.", nameof(description));
        }

        return WithLockedVersionAsync(null, true,
            async (context, activeVersionId) =>
            {
                if (!string.Equals(activeVersionId, Required(expectedActiveVersionId,
                        nameof(expectedActiveVersionId)), StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"The active Category Tree changed from '{expectedActiveVersionId}' to '{activeVersionId}'. Refresh before cloning.");
                }
                var outputVersionId = new SqlParameter("@VersionId", SqlDbType.NVarChar, 10)
                {
                    Direction = ParameterDirection.Output
                };

                await ExecuteProcedureAsync(context, "Cash.proc_CategoryVersionClone", cancellationToken,
                    NVarChar("@Description", 100, description.Trim()),
                    NVarCharMax("@Notes", notes),
                    outputVersionId);

                var versionId = Convert.ToString(outputVersionId.Value)
                    ?? throw new InvalidOperationException("The clone command did not return a version identifier.");
                return await ReadCommandResultAsync(context, versionId, cancellationToken);
            }, cancellationToken);
    }

    private static void AddComparisonGroup<T>(ICollection<CategoryTreeChangeGroup> groups,
        string label, IEnumerable<T> rows, string includeVersionId, string excludeVersionId,
        Func<T, string> versionId, Func<T, string> key)
    {
        var included = rows.Where(item => versionId(item) == includeVersionId).Select(key);
        var excluded = rows.Where(item => versionId(item) == excludeVersionId).Select(key);
        AddGroup(groups, label, included.Except(excluded, StringComparer.Ordinal));
    }

    private static void AddGroup(ICollection<CategoryTreeChangeGroup> groups, string label,
        IEnumerable<string> items)
    {
        var ordered = items.Distinct(StringComparer.Ordinal).OrderBy(item => item,
            StringComparer.Ordinal).ToArray();
        if (ordered.Length > 0)
        {
            groups.Add(new(label, ordered));
        }
    }

    private static bool CategoryContentEquals(Cash_tbCategory left, Cash_tbCategory right) =>
        left.Category == right.Category
        && left.CategoryTypeCode == right.CategoryTypeCode
        && left.CashPolarityCode == right.CashPolarityCode
        && left.CashTypeCode == right.CashTypeCode
        && left.DisplayOrder == right.DisplayOrder
        && left.IsEnabled == right.IsEnabled;

    private static string RootName(short code) => code switch
    {
        (short)NodeEnum.CategoryRootType.NetProfit => "Net Profit",
        (short)NodeEnum.CategoryRootType.Vat => "VAT",
        _ => $"Root {code}"
    };

    public Task<CategoryTreeCommandResult> RefreshDraftAsync(string versionId, string changeNote,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(changeNote, nameof(changeNote));
        return ExecuteVersionProcedureAsync(versionId,
            "Cash.proc_CategoryVersionRefresh", cancellationToken,
            NVarChar("@VersionId", 10, versionId),
            NVarCharMax("@ChangeNote", changeNote.Trim()));
    }

    public Task<CategoryTreeCommandResult> UpdateMetadataAsync(string versionId, string description,
        string? notes, string? changeNote,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(description, nameof(description));
        return ExecuteVersionProcedureAsync(versionId,
            "Cash.proc_CategoryVersionNotesSet", cancellationToken,
            NVarChar("@VersionId", 10, versionId),
            NVarChar("@Description", 100, description.Trim()),
            NVarCharMax("@Notes", notes),
            NVarCharMax("@ChangeNote", changeNote));
    }

    public Task<CategoryTreeCommandResult> ValidateAsync(string versionId,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        return WithLockedVersionAsync(versionId, false,
            async (context, lockedVersionId) =>
            {
                var digest = new SqlParameter("@CanonicalSha256", SqlDbType.Binary, 32)
                {
                    Direction = ParameterDirection.Output
                };
                var byteLength = new SqlParameter("@CanonicalByteLength", SqlDbType.BigInt)
                {
                    Direction = ParameterDirection.Output
                };

                await ExecuteProcedureAsync(context, "Cash.proc_CategoryVersionValidate", cancellationToken,
                    NVarChar("@VersionId", 10, lockedVersionId), digest, byteLength);

                var result = await ReadCommandResultAsync(context, lockedVersionId, cancellationToken);
                return result with
                {
                    CanonicalSha256 = digest.Value is byte[] bytes ? Convert.ToHexString(bytes) : null,
                    CanonicalByteLength = byteLength.Value is DBNull ? null : Convert.ToInt64(byteLength.Value)
                };
            }, cancellationToken);
    }

    public Task<CategoryTreeCommandResult> PublishAsync(string versionId, string changeNote,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(changeNote, nameof(changeNote));
        return ExecuteVersionProcedureAsync(versionId,
            "Cash.proc_CategoryVersionPublish", cancellationToken,
            NVarChar("@VersionId", 10, versionId),
            NVarCharMax("@ChangeNote", changeNote.Trim()));
    }

    public Task<CategoryTreeCommandResult> DiscardDraftAsync(string versionId,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        return WithLockedVersionAsync(versionId, false,
            async (context, lockedVersionId) =>
            {
                await ExecuteProcedureAsync(context, "Cash.proc_CategoryVersionDiscard", cancellationToken,
                    NVarChar("@VersionId", 10, lockedVersionId));
                return new CategoryTreeCommandResult(lockedVersionId, null);
            }, cancellationToken);
    }

    public Task<CategoryTreeCommandResult> SaveCategoryAsync(CategoryTreeCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(command.CategoryCode, nameof(command.CategoryCode));
        Required(command.Category, nameof(command.Category));
        return ExecuteVersionProcedureAsync(command.VersionId,
            "Cash.proc_CategorySave", cancellationToken,
            NVarChar("@VersionId", 10, command.VersionId),
            NVarChar("@CategoryCode", 10, command.CategoryCode),
            NVarChar("@Category", 50, command.Category),
            SmallInt("@CategoryTypeCode", command.CategoryTypeCode),
            SmallInt("@CashPolarityCode", command.CashPolarityCode),
            SmallInt("@CashTypeCode", command.CashTypeCode),
            SmallInt("@DisplayOrder", command.DisplayOrder),
            SmallInt("@IsEnabled", command.IsEnabled),
            NVarCharMax("@ChangeNote", command.ChangeNote));
    }

    public Task<CategoryTreeCommandResult> ApplyBatchAsync(CategoryTreeMutationBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        return WithLockedVersionAsync(command.VersionId, false,
            async (context, versionId) =>
            {
                var eventBoundary = await ReadLatestEditEventIdAsync(context, versionId,
                    cancellationToken);

                foreach (var category in command.Categories)
                {
                    if (!string.Equals(category.VersionId, versionId, StringComparison.Ordinal))
                    {
                        throw new ArgumentException("Every category in a batch must use the batch version.");
                    }

                    await SaveCategoryCoreAsync(context, category, cancellationToken);
                }

                foreach (var total in command.Totals)
                {
                    if (!string.Equals(total.VersionId, versionId, StringComparison.Ordinal))
                    {
                        throw new ArgumentException("Every total in a batch must use the batch version.");
                    }

                    await SaveTotalCoreAsync(context, total, cancellationToken);
                }

                foreach (var move in command.CashCodeMoves)
                {
                    Required(move.CashCode, nameof(move.CashCode));
                    Required(move.CategoryCode, nameof(move.CategoryCode));
                    await ExecuteProcedureAsync(context, "Cash.proc_CategoryCodeMapMove", cancellationToken,
                        NVarChar("@VersionId", 10, versionId),
                        NVarChar("@CashCode", 50, move.CashCode),
                        NVarChar("@CategoryCode", 10, move.CategoryCode),
                        NVarCharMax("@ChangeNote", move.ChangeNote));
                }

                foreach (var categoryCode in command.DeletedCategoryCodes ?? Array.Empty<string>())
                {
                    Required(categoryCode, nameof(categoryCode));
                    await ExecuteProcedureAsync(context, "Cash.proc_CategoryDelete", cancellationToken,
                        NVarChar("@VersionId", 10, versionId),
                        NVarChar("@CategoryCode", 10, categoryCode),
                        NVarCharMax("@ChangeNote", "Category deleted."));
                }

                await CoalesceBatchEditEventsAsync(context, versionId, eventBoundary,
                    BatchChangeNote(command), cancellationToken);

                return await ReadCommandResultAsync(context, versionId, cancellationToken);
            }, cancellationToken);
    }

    public Task<CategoryTreeCommandResult> SaveExpressionAsync(CategoryTreeExpressionCommand command,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        return WithLockedVersionAsync(command.Category.VersionId, false,
            async (context, versionId) =>
            {
                await SaveCategoryCoreAsync(context, command.Category, cancellationToken);

                var expression = await context.Cash_tbCategoryExps
                    .SingleOrDefaultAsync(item => item.CategoryCode == command.Category.CategoryCode,
                        cancellationToken);
                if (expression is null)
                {
                    expression = new Cash_tbCategoryExp
                    {
                        CategoryCode = command.Category.CategoryCode,
                        IsError = false
                    };
                    context.Cash_tbCategoryExps.Add(expression);
                }

                expression.Expression = command.Expression;
                expression.Format = command.Format;
                expression.SyntaxTypeCode = command.SyntaxTypeCode;
                await context.SaveChangesAsync(cancellationToken);
                return await ReadCommandResultAsync(context, versionId, cancellationToken);
            }, cancellationToken);
    }

    public Task<CategoryTreeCommandResult> DeleteCategoryAsync(string versionId, string categoryCode,
        string? changeNote, CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(categoryCode, nameof(categoryCode));
        return ExecuteVersionProcedureAsync(versionId,
            "Cash.proc_CategoryDelete", cancellationToken,
            NVarChar("@VersionId", 10, versionId),
            NVarChar("@CategoryCode", 10, categoryCode),
            NVarCharMax("@ChangeNote", changeNote));
    }

    public Task<CategoryTreeCommandResult> SaveTotalAsync(CategoryTreeTotalCommand command,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(command.ParentCode, nameof(command.ParentCode));
        Required(command.ChildCode, nameof(command.ChildCode));
        return ExecuteVersionProcedureAsync(command.VersionId,
            "Cash.proc_CategoryTotalSave", cancellationToken,
            NVarChar("@VersionId", 10, command.VersionId),
            NVarChar("@ParentCode", 10, command.ParentCode),
            NVarChar("@ChildCode", 10, command.ChildCode),
            SmallInt("@DisplayOrder", command.DisplayOrder),
            Bit("@IsDelete", command.IsDelete),
            NVarCharMax("@ChangeNote", command.ChangeNote));
    }

    public Task<CategoryTreeCommandResult> MoveCashCodeAsync(string versionId, string cashCode,
        string categoryCode, string? changeNote,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(cashCode, nameof(cashCode));
        Required(categoryCode, nameof(categoryCode));
        return ExecuteVersionProcedureAsync(versionId,
            "Cash.proc_CategoryCodeMapMove", cancellationToken,
            NVarChar("@VersionId", 10, versionId),
            NVarChar("@CashCode", 50, cashCode),
            NVarChar("@CategoryCode", 10, categoryCode),
            NVarCharMax("@ChangeNote", changeNote));
    }

    public Task<CategoryTreeCommandResult> SetRootAsync(string versionId, short rootTypeCode,
        string categoryCode, string? changeNote,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(categoryCode, nameof(categoryCode));
        return ExecuteVersionProcedureAsync(versionId,
            "Cash.proc_CategoryRootSet", cancellationToken,
            NVarChar("@VersionId", 10, versionId),
            SmallInt("@RootTypeCode", rootTypeCode),
            NVarChar("@CategoryCode", 10, categoryCode),
            NVarCharMax("@ChangeNote", changeNote));
    }

    public Task<CategoryTreeCommandResult> CreateCashCodeAsync(CategoryTreeCashCodeCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(command.CashCode, nameof(command.CashCode));
        Required(command.CashDescription, nameof(command.CashDescription));
        Required(command.CategoryCode, nameof(command.CategoryCode));
        Required(command.TaxCode, nameof(command.TaxCode));
        return ExecuteVersionProcedureAsync(command.VersionId,
            "Cash.proc_CodeAdd", cancellationToken,
            NVarChar("@VersionId", 10, command.VersionId),
            NVarChar("@CashCode", 50, command.CashCode),
            NVarChar("@CashDescription", 100, command.CashDescription),
            NVarChar("@CategoryCode", 10, command.CategoryCode),
            NVarChar("@TaxCode", 10, command.TaxCode),
            SmallInt("@IsEnabled", command.IsEnabled));
    }

    public Task<CategoryTreeCommandResult> UpdateCashCodeAsync(CategoryTreeCashCodeUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(command.CashCode, nameof(command.CashCode));
        Required(command.CashDescription, nameof(command.CashDescription));
        Required(command.TaxCode, nameof(command.TaxCode));
        return WithLockedVersionAsync(command.VersionId, false,
            async (context, versionId) =>
            {
                await EnsureActivePublishedVersionAsync(context, versionId, cancellationToken);
                var code = await context.Cash_tbCodes
                    .SingleOrDefaultAsync(item => item.CashCode == command.CashCode, cancellationToken)
                    ?? throw new InvalidOperationException($"Cash Code '{command.CashCode}' was not found.");
                code.CashDescription = command.CashDescription;
                code.TaxCode = command.TaxCode;
                code.IsEnabled = command.IsEnabled;
                await context.SaveChangesAsync(cancellationToken);
                return await ReadCommandResultAsync(context, versionId, cancellationToken);
            }, cancellationToken);
    }

    private static async Task EnsureEditableVersionAsync(NodeContext context, string versionId,
        CancellationToken cancellationToken)
    {
        var state = await context.Cash_tbCategoryVersions
            .AsNoTracking()
            .Where(version => version.VersionId == versionId)
            .Select(version => new
            {
                version.VersionStatusCode,
                IsActive = context.App_tbOptions.Any(options =>
                    options.CategoryVersionId == version.VersionId)
            })
            .SingleAsync(cancellationToken);

        if (state.VersionStatusCode is not (short)CategoryTreeStatus.Draft
            and not (short)CategoryTreeStatus.Published)
        {
            throw new InvalidOperationException(
                "Category Tree changes are permitted only for Draft or Published versions.");
        }

        if (state.VersionStatusCode == (short)CategoryTreeStatus.Published && !state.IsActive)
        {
            throw new InvalidOperationException("Only the active Published Category Tree can be changed.");
        }
    }

    private static async Task EnsureActivePublishedVersionAsync(NodeContext context, string versionId,
        CancellationToken cancellationToken)
    {
        var isActivePublished = await context.Cash_tbCategoryVersions
            .AsNoTracking()
            .Where(version => version.VersionId == versionId
                && version.VersionStatusCode == (short)CategoryTreeStatus.Published)
            .AnyAsync(version => context.App_tbOptions.Any(options =>
                options.CategoryVersionId == version.VersionId), cancellationToken);

        if (!isActivePublished)
        {
            throw new InvalidOperationException(
                "Cash Code master data can be changed only from the active Published Category Tree.");
        }
    }

    public Task<CategoryTreeCommandResult> DeleteCashCodeAsync(string versionId, string cashCode,
        string? changeNote,
        CancellationToken cancellationToken = default)
    {
        DemandAdministrator();
        Required(cashCode, nameof(cashCode));
        return WithLockedVersionAsync(versionId, false,
            async (context, lockedVersionId) =>
            {
                await EnsureActivePublishedVersionAsync(context, lockedVersionId, cancellationToken);
                await ExecuteProcedureAsync(context, "Cash.proc_CodeDelete", cancellationToken,
                    NVarChar("@CashCode", 50, cashCode),
                    NVarCharMax("@ChangeNote", changeNote));
                return await ReadCommandResultAsync(context, lockedVersionId, cancellationToken);
            }, cancellationToken);
    }

    private Task<CategoryTreeCommandResult> ExecuteVersionProcedureAsync(string versionId,
        string procedure, CancellationToken cancellationToken,
        params SqlParameter[] parameters) =>
        WithLockedVersionAsync(versionId, false,
            async (context, lockedVersionId) =>
            {
                await ExecuteProcedureAsync(context, procedure, cancellationToken, parameters);
                return await ReadCommandResultAsync(context, lockedVersionId, cancellationToken);
            }, cancellationToken);

    private static Task SaveCategoryCoreAsync(NodeContext context, CategoryTreeCategoryCommand command,
        CancellationToken cancellationToken)
    {
        Required(command.CategoryCode, nameof(command.CategoryCode));
        Required(command.Category, nameof(command.Category));
        return ExecuteProcedureAsync(context, "Cash.proc_CategorySave", cancellationToken,
            NVarChar("@VersionId", 10, command.VersionId),
            NVarChar("@CategoryCode", 10, command.CategoryCode),
            NVarChar("@Category", 50, command.Category),
            SmallInt("@CategoryTypeCode", command.CategoryTypeCode),
            SmallInt("@CashPolarityCode", command.CashPolarityCode),
            SmallInt("@CashTypeCode", command.CashTypeCode),
            SmallInt("@DisplayOrder", command.DisplayOrder),
            SmallInt("@IsEnabled", command.IsEnabled),
            NVarCharMax("@ChangeNote", command.ChangeNote));
    }

    private static Task SaveTotalCoreAsync(NodeContext context, CategoryTreeTotalCommand command,
        CancellationToken cancellationToken)
    {
        Required(command.ParentCode, nameof(command.ParentCode));
        Required(command.ChildCode, nameof(command.ChildCode));
        return ExecuteProcedureAsync(context, "Cash.proc_CategoryTotalSave", cancellationToken,
            NVarChar("@VersionId", 10, command.VersionId),
            NVarChar("@ParentCode", 10, command.ParentCode),
            NVarChar("@ChildCode", 10, command.ChildCode),
            SmallInt("@DisplayOrder", command.DisplayOrder),
            Bit("@IsDelete", command.IsDelete),
            NVarCharMax("@ChangeNote", command.ChangeNote));
    }

    private static async Task<long> ReadLatestEditEventIdAsync(NodeContext context, string versionId,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(context,
            "SELECT COALESCE(MAX(VersionEventId), 0) FROM Cash.tbCategoryVersionEvent " +
            "WHERE VersionId = @VersionId AND EventTypeCode = 1;");
        command.Parameters.Add(NVarChar("@VersionId", 10, versionId));
        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task CoalesceBatchEditEventsAsync(NodeContext context, string versionId,
        long eventBoundary, string changeNote, CancellationToken cancellationToken)
    {
        const string sql = """
            DECLARE @FromStatusCode SMALLINT,
                    @ToStatusCode SMALLINT,
                    @UserId NVARCHAR(10),
                    @UserName NVARCHAR(50);

            SELECT TOP (1)
                   @FromStatusCode = FromStatusCode,
                   @ToStatusCode = ToStatusCode,
                   @UserId = UserId,
                   @UserName = UserName
            FROM Cash.tbCategoryVersionEvent
            WHERE VersionId = @VersionId
              AND EventTypeCode = 1
              AND VersionEventId > @EventBoundary
            ORDER BY VersionEventId;

            IF @UserId IS NOT NULL
            BEGIN
                DELETE FROM Cash.tbCategoryVersionEvent
                WHERE VersionId = @VersionId
                  AND EventTypeCode = 1
                  AND VersionEventId > @EventBoundary;

                INSERT Cash.tbCategoryVersionEvent
                    (VersionId, EventTypeCode, FromStatusCode, ToStatusCode,
                     UserId, UserName, ChangeNote)
                VALUES
                    (@VersionId, 1, @FromStatusCode, @ToStatusCode,
                     @UserId, @UserName, @ChangeNote);
            END;
            """;

        await using var command = CreateCommand(context, sql);
        command.Parameters.Add(NVarChar("@VersionId", 10, versionId));
        command.Parameters.Add(new SqlParameter("@EventBoundary", SqlDbType.BigInt)
        {
            Value = eventBoundary
        });
        command.Parameters.Add(NVarCharMax("@ChangeNote", changeNote));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string BatchChangeNote(CategoryTreeMutationBatchCommand command)
    {
        var changeNote = command.Categories.Select(category => category.ChangeNote)
            .Concat(command.Totals.Select(total => total.ChangeNote))
            .Concat(command.CashCodeMoves.Select(move => move.ChangeNote))
            .FirstOrDefault(note => !string.IsNullOrWhiteSpace(note));

        if (!string.IsNullOrWhiteSpace(changeNote))
        {
            return changeNote.Trim();
        }

        return command.DeletedCategoryCodes?.Count > 0
            ? "Category subtree removed."
            : "Category Tree batch updated.";
    }

    private async Task<T> WithLockedVersionAsync<T>(string? requestedVersionId, bool useActiveVersion,
        Func<NodeContext, string, Task<T>> command,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var activeVersionId = await ReadActiveVersionWithLockAsync(context, cancellationToken);
        var versionId = useActiveVersion ? activeVersionId : Required(requestedVersionId, nameof(requestedVersionId));
        await LockVersionAsync(context, versionId, cancellationToken);

        var result = await command(context, versionId);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private static async Task<string> ReadActiveVersionWithLockAsync(NodeContext context,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(context,
            "SELECT CategoryVersionId FROM App.tbOptions WITH (UPDLOCK, HOLDLOCK);");
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToString(value)
            ?? throw new InvalidOperationException("The active Category Tree version is not configured.");
    }

    private static async Task LockVersionAsync(NodeContext context, string versionId,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(context,
            "SELECT 1 FROM Cash.tbCategoryVersion WITH (UPDLOCK, HOLDLOCK) WHERE VersionId = @VersionId;");
        command.Parameters.Add(NVarChar("@VersionId", 10, versionId));
        var value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is null)
        {
            throw new InvalidOperationException($"Category Tree version '{versionId}' was not found.");
        }
    }

    private static async Task ExecuteProcedureAsync(NodeContext context, string procedure,
        CancellationToken cancellationToken, params SqlParameter[] parameters)
    {
        await using var command = CreateCommand(context, procedure);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static System.Data.Common.DbCommand CreateCommand(NodeContext context, string commandText)
    {
        var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = commandText;
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        return command;
    }

    private static async Task<CategoryTreeCommandResult> ReadCommandResultAsync(NodeContext context,
        string versionId, CancellationToken cancellationToken)
    {
        context.ChangeTracker.Clear();
        var version = await context.Cash_tbCategoryVersions
            .AsNoTracking()
            .SingleAsync(item => item.VersionId == versionId, cancellationToken);
        return new(version.VersionId, ToStatus(version.VersionStatusCode),
            ToHex(version.CanonicalSha256),
            version.CanonicalByteLength);
    }

    private static CategoryTreeVersionInfo ToVersionInfo(Cash_tbCategoryVersion version,
        string activeVersionId, IReadOnlyDictionary<string, Cash_tbCategoryVersion> byId,
        IReadOnlyDictionary<short, string> statusNames,
        IReadOnlyDictionary<string, DateTime> validationTimes, bool isAdministrator)
    {
        var status = ToStatus(version.VersionStatusCode);
        var isActive = string.Equals(version.VersionId, activeVersionId, StringComparison.Ordinal);
        var stale = PredecessorIsStale(version, byId);
        var validated = validationTimes.TryGetValue(version.VersionId, out var validatedOn)
            && validatedOn >= version.ContentChangedOn;
        var capabilities = CategoryTreeCapabilityContract.Resolve(status, isActive, isAdministrator,
            hasSubmissionLock: false, stale, validated);

        return new(
            version.VersionId,
            status,
            statusNames.GetValueOrDefault(version.VersionStatusCode, status.ToString()),
            version.Description,
            version.Notes,
            version.PredecessorVersionId,
            version.ContentChangedOn,
            version.EffectiveFromOn,
            version.EffectiveToOn,
            version.CreatedOn,
            version.CreatedByUserId,
            version.CanonicalProfile,
            ToHex(version.CanonicalSha256),
            version.CanonicalByteLength,
            isActive,
            stale,
            validated,
            capabilities);
    }

    private static bool PredecessorIsStale(Cash_tbCategoryVersion version,
        IReadOnlyDictionary<string, Cash_tbCategoryVersion> byId)
    {
        if (version.PredecessorVersionId is null)
        {
            return false;
        }

        return version.BaseContentSha256 is null
            || !byId.TryGetValue(version.PredecessorVersionId, out var predecessor)
            || predecessor.CanonicalSha256 is null
            || !CryptographicOperations.FixedTimeEquals(version.BaseContentSha256,
                predecessor.CanonicalSha256);
    }

    private static string Consequence(CategoryTreeStatus status, bool isActive, string activeVersionId) =>
        (status, isActive) switch
        {
            (CategoryTreeStatus.Published, true) =>
                "Live and editable. Changes affect current and unsubmitted statements immediately.",
            (CategoryTreeStatus.Submitted, true) =>
                "Live and read-only because this version supports a statutory submission.",
            (CategoryTreeStatus.Draft, _) =>
                $"Proposal only. Statements continue to use {activeVersionId} until this Draft is published.",
            _ => "Historical and read-only. This version is retained for reconstruction and evidence."
        };

    private void DemandAdministrator()
    {
        if (_httpContextAccessor.HttpContext?.User.IsInRole(Constants.AdministratorsRole) != true)
        {
            throw new UnauthorizedAccessException(
                "Administrator privileges are required to change Category Tree versions.");
        }
    }

    private static CategoryTreeStatus ToStatus(short statusCode) => statusCode switch
    {
        0 => CategoryTreeStatus.Draft,
        1 => CategoryTreeStatus.Published,
        2 => CategoryTreeStatus.Submitted,
        3 => CategoryTreeStatus.Archived,
        _ => throw new InvalidOperationException($"Unknown Category Tree status code {statusCode}.")
    };

    private static CategoryTreeStatus? ToNullableStatus(short? statusCode) =>
        statusCode.HasValue ? ToStatus(statusCode.Value) : null;

    private static string? ToHex(byte[]? bytes) => bytes is null ? null : Convert.ToHexString(bytes);

    private static string Required(string? value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();

    private static SqlParameter NVarChar(string name, int size, string? value) =>
        new(name, SqlDbType.NVarChar, size) { Value = value is null ? DBNull.Value : value };

    private static SqlParameter NVarCharMax(string name, string? value) =>
        new(name, SqlDbType.NVarChar, -1) { Value = value is null ? DBNull.Value : value };

    private static SqlParameter SmallInt(string name, short? value) =>
        new(name, SqlDbType.SmallInt) { Value = value.HasValue ? value.Value : DBNull.Value };

    private static SqlParameter Bit(string name, bool value) =>
        new(name, SqlDbType.Bit) { Value = value };
}
