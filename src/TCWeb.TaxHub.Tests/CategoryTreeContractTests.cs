using TradeControl.Web.AppServices.CategoryTree;

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
