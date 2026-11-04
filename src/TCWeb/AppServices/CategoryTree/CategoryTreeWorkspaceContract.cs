using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CategoryTreeStatus = TradeControl.Web.Data.NodeEnum.CategoryVersionStatus;

namespace TradeControl.Web.AppServices.CategoryTree;

public sealed record CategoryTreeCapabilities(
    bool CanEditTree,
    bool CanCloneActive,
    bool CanRefreshDraft,
    bool CanValidate,
    bool CanReviewChanges,
    bool CanPublish,
    bool CanDiscard,
    bool CanViewHistory,
    bool CanSelectForCashStatement);

public static class CategoryTreeCapabilityContract
{
    public static CategoryTreeCapabilities Resolve(CategoryTreeStatus status, bool isActive,
        bool isAdministrator, bool hasSubmissionLock, bool predecessorIsStale,
        bool hasCurrentValidation)
    {
        var isEditablePublished = status == CategoryTreeStatus.Published && isActive && !hasSubmissionLock;
        var isEditableDraft = status == CategoryTreeStatus.Draft;
        var canEdit = isAdministrator && (isEditablePublished || isEditableDraft);

        return new(
            CanEditTree: canEdit,
            CanCloneActive: isAdministrator && isActive
                && status is CategoryTreeStatus.Published or CategoryTreeStatus.Submitted
                && !hasSubmissionLock,
            CanRefreshDraft: isAdministrator && isEditableDraft && predecessorIsStale,
            CanValidate: isAdministrator && (isEditablePublished || isEditableDraft),
            CanReviewChanges: true,
            CanPublish: isAdministrator && isEditableDraft && !predecessorIsStale
                && hasCurrentValidation && !hasSubmissionLock,
            CanDiscard: isAdministrator && isEditableDraft,
            CanViewHistory: true,
            CanSelectForCashStatement: status is not CategoryTreeStatus.Draft);
    }
}

public sealed record CategoryTreeVersionInfo(
    string VersionId,
    CategoryTreeStatus Status,
    string StatusName,
    string Description,
    string? Notes,
    string? PredecessorVersionId,
    DateTime ContentChangedOn,
    DateTime? EffectiveFromOn,
    DateTime? EffectiveToOn,
    DateTime CreatedOn,
    string CreatedByUserId,
    string CanonicalProfile,
    string? CanonicalSha256,
    long? CanonicalByteLength,
    bool IsActive,
    bool PredecessorIsStale,
    bool HasCurrentValidation,
    CategoryTreeCapabilities Capabilities);

public sealed record CategoryTreeVersionEventInfo(
    long VersionEventId,
    string VersionId,
    short EventTypeCode,
    CategoryTreeStatus? FromStatus,
    CategoryTreeStatus? ToStatus,
    DateTime EventOn,
    string UserId,
    string UserName,
    string? ChangeNote,
    string? ContentSha256);

public sealed record CategoryTreeWorkspaceContext(
    string ActiveVersionId,
    CategoryTreeVersionInfo SelectedVersion,
    IReadOnlyList<CategoryTreeVersionInfo> Versions,
    IReadOnlyList<CategoryTreeVersionEventInfo> SelectedVersionEvents,
    string ConsequenceStatement,
    string? Notice);

public sealed record CategoryTreeCommandResult(
    string VersionId,
    CategoryTreeStatus? Status,
    string? CanonicalSha256 = null,
    long? CanonicalByteLength = null);

public sealed record CategoryTreeChangeGroup(
    string Label,
    IReadOnlyList<string> Items);

public sealed record CategoryTreeVersionReview(
    string VersionId,
    string? PredecessorVersionId,
    IReadOnlyList<CategoryTreeChangeGroup> Groups,
    IReadOnlyList<string> Warnings)
{
    public bool HasChanges => Groups.Count > 0;
}

public sealed record CategoryTreeCategoryCommand(
    string VersionId,
    string CategoryCode,
    string Category,
    short CategoryTypeCode,
    short CashPolarityCode,
    short CashTypeCode,
    short DisplayOrder,
    short IsEnabled,
    string? ChangeNote);

public sealed record CategoryTreeTotalCommand(
    string VersionId,
    string ParentCode,
    string ChildCode,
    short DisplayOrder,
    bool IsDelete,
    string? ChangeNote);

public sealed record CategoryTreeCashCodeCreateCommand(
    string VersionId,
    string CashCode,
    string CashDescription,
    string CategoryCode,
    string TaxCode,
    short IsEnabled);

public sealed record CategoryTreeCashCodeMoveCommand(
    string CashCode,
    string CategoryCode,
    string? ChangeNote);

public sealed record CategoryTreeMutationBatchCommand(
    string VersionId,
    IReadOnlyList<CategoryTreeCategoryCommand> Categories,
    IReadOnlyList<CategoryTreeTotalCommand> Totals,
    IReadOnlyList<CategoryTreeCashCodeMoveCommand> CashCodeMoves,
    IReadOnlyList<string>? DeletedCategoryCodes = null);

public sealed record CategoryTreeExpressionCommand(
    CategoryTreeCategoryCommand Category,
    string Expression,
    string Format,
    short SyntaxTypeCode);

public sealed record CategoryTreeCashCodeUpdateCommand(
    string VersionId,
    string CashCode,
    string CashDescription,
    string TaxCode,
    short IsEnabled);

public interface ICategoryTreeChangeControlService
{
    Task<CategoryTreeWorkspaceContext> GetWorkspaceAsync(string? requestedVersionId,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeVersionReview> GetReviewAsync(string versionId,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> CloneActiveAsync(string expectedActiveVersionId,
        string description, string? notes,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> RefreshDraftAsync(string versionId, string changeNote,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> UpdateMetadataAsync(string versionId, string description,
        string? notes, string? changeNote,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> ValidateAsync(string versionId,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> PublishAsync(string versionId, string changeNote,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> DiscardDraftAsync(string versionId,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> SaveCategoryAsync(CategoryTreeCategoryCommand command,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> ApplyBatchAsync(CategoryTreeMutationBatchCommand command,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> SaveExpressionAsync(CategoryTreeExpressionCommand command,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> DeleteCategoryAsync(string versionId, string categoryCode,
        string? changeNote, CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> SaveTotalAsync(CategoryTreeTotalCommand command,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> MoveCashCodeAsync(string versionId, string cashCode,
        string categoryCode, string? changeNote,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> SetRootAsync(string versionId, short rootTypeCode,
        string categoryCode, string? changeNote,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> CreateCashCodeAsync(CategoryTreeCashCodeCreateCommand command,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> UpdateCashCodeAsync(CategoryTreeCashCodeUpdateCommand command,
        CancellationToken cancellationToken = default);

    Task<CategoryTreeCommandResult> DeleteCashCodeAsync(string versionId, string cashCode,
        string? changeNote,
        CancellationToken cancellationToken = default);
}
