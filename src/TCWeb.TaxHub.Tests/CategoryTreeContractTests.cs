using TradeControl.Web.AppServices.CategoryTree;
using TradeControl.Web.Data;
using CategoryTreeStatus = TradeControl.Web.Data.NodeEnum.CategoryVersionStatus;

internal static class CategoryTreeContractTests
{
    public static int Run()
    {
        var assertions = 0;
        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
        var source = Vector();
        Assert((short)NodeEnum.CategoryVersionStatus.Draft == 0
            && (short)NodeEnum.CategoryVersionStatus.Published == 1
            && (short)NodeEnum.CategoryVersionStatus.Submitted == 2
            && (short)NodeEnum.CategoryVersionStatus.Archived == 3,
            "Category Tree version-status enum values diverged from the SQL vocabulary.");
        Assert((short)NodeEnum.CategoryVersionEventType.Create == 0
            && (short)NodeEnum.CategoryVersionEventType.Edit == 1
            && (short)NodeEnum.CategoryVersionEventType.Validate == 2
            && (short)NodeEnum.CategoryVersionEventType.Publish == 3
            && (short)NodeEnum.CategoryVersionEventType.Submit == 4
            && (short)NodeEnum.CategoryVersionEventType.Archive == 5
            && (short)NodeEnum.CategoryVersionEventType.RefreshOrRebase == 6,
            "Category Tree version-event enum values diverged from the SQL contract.");
        Assert((short)NodeEnum.CategoryRootType.NetProfit == 0
            && (short)NodeEnum.CategoryRootType.Vat == 1,
            "Category Tree root-type enum values diverged from the SQL vocabulary.");
        var canonical = CategoryTreeCanonicalizer.Create(source);
        Assert(canonical.ByteLength == 585,
            $"The Category Tree canonical byte length changed: {canonical.ByteLength}.");
        Assert(canonical.Sha256 == "8B77B246D3DFDB77230B52695747D5D31A135FC5FFB65CD89CE4844A6D032177",
            $"The Category Tree v1 canonical conformance vector changed: {canonical.Sha256}.");
        Assert(CategoryTreeCanonicalizer.Create(source with
        {
            Categories = source.Categories.Reverse().ToArray(), Roots = source.Roots.Reverse().ToArray()
        }).Bytes.SequenceEqual(canonical.Bytes), "Canonical bytes depend on input row order.");
        var changed = source with
        {
            Categories = source.Categories.Select(x => x.CategoryCode == "CAT-1"
                ? x with { IsEnabled = false } : x).ToArray()
        };
        Assert(CategoryTreeCanonicalizer.Create(changed).Sha256 != canonical.Sha256,
            "A semantic change did not change the Category Tree digest.");
        Assert(CategoryTreeCanonicalizer.Create(source with
        {
            CodeMap = [new("CC-SALES", "ROOT-1")]
        }).Sha256 != canonical.Sha256, "A Cash Code mapping change did not change the Category Tree digest.");
        Assert(CategoryTreeCanonicalizer.Create(source with
        {
            Categories = source.Categories.Select(x => x.CategoryCode == "CAT-1"
                ? x with { Category = "Cafe\u0301" } : x).ToArray()
        }).Bytes.SequenceEqual(CategoryTreeCanonicalizer.Create(source with
        {
            Categories = source.Categories.Select(x => x.CategoryCode == "CAT-1"
                ? x with { Category = "Caf\u00e9" } : x).ToArray()
        }).Bytes), "NFC-equivalent content produced different bytes.");
        Assert(CategoryTreeLifecycleContract.CanBeSelectedAsActive(CategoryTreeStatus.Published)
            && CategoryTreeLifecycleContract.CanBeSelectedAsActive(CategoryTreeStatus.Submitted)
            && !CategoryTreeLifecycleContract.CanBeSelectedAsActive(CategoryTreeStatus.Draft)
            && !CategoryTreeLifecycleContract.CanBeSelectedAsActive(CategoryTreeStatus.Archived)
            && CategoryTreeLifecycleContract.IsActive(CategoryTreeStatus.Published, true)
            && CategoryTreeLifecycleContract.IsActive(CategoryTreeStatus.Submitted, true)
            && !CategoryTreeLifecycleContract.IsActive(CategoryTreeStatus.Published, false)
            && !CategoryTreeLifecycleContract.IsActive(CategoryTreeStatus.Draft, true),
            "The options pointer or active-status boundary changed.");
        Assert(CategoryTreeLifecycleContract.CanTransition(CategoryTreeStatus.Draft, CategoryTreeStatus.Published)
            && CategoryTreeLifecycleContract.CanTransition(CategoryTreeStatus.Published, CategoryTreeStatus.Submitted)
            && CategoryTreeLifecycleContract.CanTransition(CategoryTreeStatus.Published, CategoryTreeStatus.Archived)
            && CategoryTreeLifecycleContract.CanTransition(CategoryTreeStatus.Submitted, CategoryTreeStatus.Archived),
            "A required lifecycle transition is unavailable.");
        Assert(!CategoryTreeLifecycleContract.CanTransition(CategoryTreeStatus.Submitted, CategoryTreeStatus.Published)
            && !CategoryTreeLifecycleContract.CanTransition(CategoryTreeStatus.Archived, CategoryTreeStatus.Draft),
            "An illegal lifecycle transition was accepted.");
        Assert(CategoryTreeLifecycleContract.PredecessorStillMatches(canonical.Sha256, canonical.Sha256)
            && !CategoryTreeLifecycleContract.PredecessorStillMatches(canonical.Sha256,
                CategoryTreeCanonicalizer.Create(changed).Sha256),
            "Draft base-content concurrency is not enforced.");

        var published = CategoryTreeCapabilityContract.Resolve(CategoryTreeStatus.Published,
            isActive: true, isAdministrator: true, hasSubmissionLock: false,
            predecessorIsStale: false, hasCurrentValidation: false);
        Assert(published.CanEditTree && published.CanCloneActive && published.CanValidate
            && !published.CanPublish && !published.CanDiscard && published.CanSelectForCashStatement,
            "The active Published capability boundary changed.");

        var submitted = CategoryTreeCapabilityContract.Resolve(CategoryTreeStatus.Submitted,
            isActive: true, isAdministrator: true, hasSubmissionLock: false,
            predecessorIsStale: false, hasCurrentValidation: true);
        Assert(!submitted.CanEditTree && submitted.CanCloneActive && !submitted.CanValidate
            && !submitted.CanPublish && submitted.CanSelectForCashStatement,
            "The active Submitted capability boundary changed.");

        var draft = CategoryTreeCapabilityContract.Resolve(CategoryTreeStatus.Draft,
            isActive: false, isAdministrator: true, hasSubmissionLock: false,
            predecessorIsStale: false, hasCurrentValidation: true);
        Assert(draft.CanEditTree && !draft.CanCloneActive && !draft.CanRefreshDraft
            && draft.CanValidate && draft.CanPublish && draft.CanDiscard
            && !draft.CanSelectForCashStatement,
            "The validated Draft capability boundary changed.");

        var staleDraft = CategoryTreeCapabilityContract.Resolve(CategoryTreeStatus.Draft,
            isActive: false, isAdministrator: true, hasSubmissionLock: false,
            predecessorIsStale: true, hasCurrentValidation: true);
        Assert(staleDraft.CanEditTree && staleDraft.CanRefreshDraft && !staleDraft.CanPublish,
            "A stale Draft can be published or cannot be explicitly refreshed.");

        var lockedPublished = CategoryTreeCapabilityContract.Resolve(CategoryTreeStatus.Published,
            isActive: true, isAdministrator: true, hasSubmissionLock: true,
            predecessorIsStale: false, hasCurrentValidation: true);
        Assert(!lockedPublished.CanEditTree && !lockedPublished.CanCloneActive
            && !lockedPublished.CanValidate,
            "A submission-locked Published version remains mutable.");

        var ordinaryUser = CategoryTreeCapabilityContract.Resolve(CategoryTreeStatus.Published,
            isActive: true, isAdministrator: false, hasSubmissionLock: false,
            predecessorIsStale: false, hasCurrentValidation: true);
        Assert(!ordinaryUser.CanEditTree && !ordinaryUser.CanCloneActive
            && !ordinaryUser.CanValidate && ordinaryUser.CanViewHistory,
            "A non-administrator received Category Tree mutation capabilities.");
        return assertions;
    }

    private static CategoryTreeContentContract Vector() => new(
        [new("ROOT-1", "Net profit", 1, 2, 0, 1, true),
         new("CAT-1", "Sales & services", 0, 1, 0, 10, true)],
        [new("ROOT-1", "CAT-1", 1)],
        [new("CC-SALES", "CAT-1")],
        [new(CategoryTreeCanonicalizer.VatRoot, "ROOT-1"),
         new(CategoryTreeCanonicalizer.NetProfitRoot, "ROOT-1")]);
}
