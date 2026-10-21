using TradeControl.Web.Tenancy;

internal static class TenantFileProviderTests
{
    public static int Run()
    {
        var assertions = 0;
        void Assert(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        var testRoot = Path.Combine(Path.GetTempPath(), $"tradecontrol-tenant-files-{Guid.NewGuid():N}");
        var seedRoot = Path.Combine(testRoot, "seed");
        var storageRoot = Path.Combine(testRoot, "storage");
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        try
        {
            var seedImages = Path.Combine(seedRoot, "content", "images");
            Directory.CreateDirectory(seedImages);
            File.WriteAllText(Path.Combine(seedImages, "logo.png"), "default");
            File.WriteAllText(Path.Combine(seedImages, "logo.png.br"), "compressed-sidecar");

            using var providerA = new TenantFileProvider(storageRoot, seedRoot, tenantA);
            using var providerB = new TenantFileProvider(storageRoot, seedRoot, tenantB);

            Assert(!string.Equals(providerA.RootPath, providerB.RootPath, StringComparison.OrdinalIgnoreCase),
                "Two tenants received the same physical file root.");
            Assert(providerA.GetFileInfo("content/images/logo.png").Exists
                && providerB.GetFileInfo("content/images/logo.png").Exists,
                "Packaged defaults were not seeded independently for both tenants.");
            Assert(!providerA.GetFileInfo("content/images/logo.png.br").Exists
                && !providerB.GetFileInfo("content/images/logo.png.br").Exists,
                "Published compression sidecars were exposed as tenant files.");

            var legacySidecar = Path.Combine(providerA.RootPath, "content", "images", "logo.png.gz");
            File.WriteAllText(legacySidecar, "legacy-compressed-sidecar");
            Assert(!providerA.GetFileInfo("content/images/logo.png.gz").Exists
                && providerA.GetDirectoryContents("content/images")
                    .All(file => !file.Name.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)),
                "A previously seeded compression sidecar remained visible to file synchronization.");

            File.WriteAllText(providerA.GetFileInfo("content/images/logo.png").PhysicalPath!, "tenant-a");
            Assert(File.ReadAllText(providerB.GetFileInfo("content/images/logo.png").PhysicalPath!) == "default",
                "A tenant file change crossed the physical tenant boundary.");

            File.Delete(providerA.GetFileInfo("content/images/logo.png").PhysicalPath!);
            using var restartedProviderA = new TenantFileProvider(storageRoot, seedRoot, tenantA);
            Assert(!restartedProviderA.GetFileInfo("content/images/logo.png").Exists,
                "A tenant deletion was silently overwritten by packaged defaults after restart.");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }

        return assertions;
    }
}
