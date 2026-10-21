using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace TradeControl.Web.Tenancy;

public sealed class TenantFileProvider : IFileProvider, IDisposable
{
    private const string SeedMarkerName = ".tradecontrol-seeded";
    private static readonly string[] ContentFolders = ["documents", "images", "templates"];
    private readonly PhysicalFileProvider _provider;

    public TenantFileProvider(string storageRoot, string seedRoot, Guid tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(seedRoot);

        RootPath = Path.Combine(Path.GetFullPath(storageRoot), tenantId.ToString("N"));
        SeedContent(RootPath, Path.GetFullPath(seedRoot));
        _provider = new PhysicalFileProvider(RootPath);
    }

    public string RootPath { get; }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        var contents = _provider.GetDirectoryContents(subpath);
        return contents.Exists
            ? new FilteredDirectoryContents(contents.Where(file => !IsCompressionSidecar(file.Name)))
            : contents;
    }

    public IFileInfo GetFileInfo(string subpath) => IsCompressionSidecar(subpath)
        ? new NotFoundFileInfo(subpath)
        : _provider.GetFileInfo(subpath);

    public Microsoft.Extensions.Primitives.IChangeToken Watch(string filter) =>
        _provider.Watch(filter);

    public void Dispose() => _provider.Dispose();

    private static void SeedContent(string tenantRoot, string seedRoot)
    {
        Directory.CreateDirectory(tenantRoot);
        var markerPath = Path.Combine(tenantRoot, SeedMarkerName);

        if (File.Exists(markerPath))
            return;

        foreach (var folder in ContentFolders)
        {
            var sourceDirectory = Path.Combine(seedRoot, "content", folder);
            var targetDirectory = Path.Combine(tenantRoot, "content", folder);
            Directory.CreateDirectory(targetDirectory);

            if (!Directory.Exists(sourceDirectory))
                continue;

            foreach (var sourcePath in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                if (IsCompressionSidecar(sourcePath))
                    continue;

                var targetPath = Path.Combine(targetDirectory, Path.GetFileName(sourcePath));

                try
                {
                    File.Copy(sourcePath, targetPath, overwrite: false);
                }
                catch (IOException) when (File.Exists(targetPath))
                {
                    // Concurrent first access may have copied the same packaged default.
                }
            }
        }

        try
        {
            using var marker = new FileStream(markerPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        }
        catch (IOException) when (File.Exists(markerPath))
        {
            // Another request completed the same idempotent seed operation.
        }
    }

    private static bool IsCompressionSidecar(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Equals(".br", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".gz", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FilteredDirectoryContents(IEnumerable<IFileInfo> files) : IDirectoryContents
    {
        private readonly IReadOnlyList<IFileInfo> _files = files.ToList();

        public bool Exists => true;

        public IEnumerator<IFileInfo> GetEnumerator() => _files.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
