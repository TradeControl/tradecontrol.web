using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;
using TradeControl.Web.Models;
using TradeControl.Web.Pages.Subject.Browser;

namespace TradeControl.Web.AppServices
{
    public sealed class SubjectBrowserService : ISubjectBrowserService
    {
        private readonly NodeContext _nodeContext;
        private readonly SemaphoreSlim _dbGate = new(initialCount: 1, maxCount: 1);
        private SubjectSnapshot? _snapshot;

        public SubjectBrowserService(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public async Task<SubjectBrowserPageResult<SubjectBrowserNode>> GetRootNodesAsync(
            string namespaceFilter,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var snapshot = await EnsureSnapshotAsync(cancellationToken);

            var roots = snapshot.Subjects.Values
                .Where(subject => !snapshot.ParentsByChild.ContainsKey(subject.SubjectCode))
                .Select(subject => CreateNode(snapshot, subject, subject.SubjectCode, null))
                .ToList();

            if (!string.IsNullOrWhiteSpace(namespaceFilter))
            {
                var filter = namespaceFilter.Trim();
                var matchCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

                roots = roots
                    .Where(node => MatchesBranchOrDescendants(snapshot, node.SubjectCode, node.NamespacePath, filter, matchCache))
                    .ToList();
            }

            roots = roots
                .OrderBy(node => node.DisplayText, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var totalCount = roots.Count;
            var items = roots
                .Skip((Math.Max(pageNumber, 1) - 1) * Math.Max(pageSize, 1))
                .Take(Math.Max(pageSize, 1))
                .ToList();

            return new SubjectBrowserPageResult<SubjectBrowserNode> {
                Items = items,
                TotalCount = totalCount,
                HasMorePages = totalCount > pageNumber * pageSize
            };
        }

        public async Task<SubjectBrowserPageResult<SubjectBrowserNode>> GetChildNodesAsync(
            string parentSubjectCode,
            string parentNamespacePath,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var snapshot = await EnsureSnapshotAsync(cancellationToken);

            if (!snapshot.ChildrenByParent.TryGetValue(parentSubjectCode, out var namespaceRows))
            {
                return new SubjectBrowserPageResult<SubjectBrowserNode>();
            }

            var children = namespaceRows
                .OrderBy(row => row.Ordinal)
                .ThenBy(row => row.ChildSubjectCode, StringComparer.OrdinalIgnoreCase)
                .Select(row => CreateNode(
                    snapshot,
                    snapshot.Subjects[row.ChildSubjectCode],
                    $"{parentNamespacePath}.{row.ChildSubjectCode}",
                    row))
                .ToList();

            if (!string.IsNullOrWhiteSpace(parentNamespacePath) && !string.IsNullOrWhiteSpace(parentNamespacePath))
            {
                var filter = parentNamespacePath;
            }

            var totalCount = children.Count;
            var items = children
                .Skip((Math.Max(pageNumber, 1) - 1) * Math.Max(pageSize, 1))
                .Take(Math.Max(pageSize, 1))
                .ToList();

            return new SubjectBrowserPageResult<SubjectBrowserNode> {
                Items = items,
                TotalCount = totalCount,
                HasMorePages = totalCount > pageNumber * pageSize
            };
        }

        public async Task<SubjectBrowserDetailModel?> GetDetailAsync(
            string subjectCode,
            CancellationToken cancellationToken = default)
        {
            var snapshot = await EnsureSnapshotAsync(cancellationToken);

            if (!snapshot.Subjects.TryGetValue(subjectCode, out var subject))
            {
                return null;
            }

            var subjectType = snapshot.Types[subject.SubjectTypeCode];
            var subjectClass = (NodeEnum.SubjectClass)subjectType.SubjectClassCode;
            var namespacePaths = GetNamespacePaths(snapshot, subjectCode);

            var addresses = await _nodeContext.Subject_tbAddresses
                .AsNoTracking()
                .Where(address => address.SubjectCode == subjectCode)
                .OrderBy(address => address.AddressCode)
                .Select(address => new SubjectBrowserAddressItem(
                    address.AddressCode,
                    address.Address,
                    address.AddressCode == subject.AddressCode))
                .ToListAsync(cancellationToken);

            var detail = new SubjectBrowserDetailModel {
                SubjectCode = subject.SubjectCode,
                SubjectTypeCode = subject.SubjectTypeCode,
                SubjectType = subjectType.SubjectType,
                SubjectClass = subjectClass,
                Name = subject.SubjectName,
                DisplayLabel = GetDisplayLabel(snapshot, subject.SubjectCode),
                NamespacePaths = namespacePaths,
                IdentityFields = CreateIdentityFields(snapshot, subject, subjectClass),
                Addresses = addresses,
                Notes = subjectClass switch {
                    NodeEnum.SubjectClass.Structural when snapshot.Structurals.TryGetValue(subjectCode, out var structural) => structural.Notes,
                    _ => null
                }
            };

            return detail;
        }

        private async Task<SubjectSnapshot> EnsureSnapshotAsync(CancellationToken cancellationToken)
        {
            if (_snapshot is not null)
            {
                return _snapshot;
            }

            await _dbGate.WaitAsync(cancellationToken);

            try
            {
                if (_snapshot is not null)
                {
                    return _snapshot;
                }

                var subjects = await _nodeContext.Subject_tbSubjects
                    .AsNoTracking()
                    .Select(subject => new SubjectRecord(
                        subject.SubjectCode,
                        subject.SubjectName,
                        subject.SubjectTypeCode,
                        subject.AddressCode,
                        subject.TaxCode,
                        subject.PaymentTerms,
                        subject.ExpectedDays,
                        subject.PaymentDays,
                        subject.PayDaysFromMonthEnd,
                        subject.PayBalance,
                        subject.OpeningBalance,
                        subject.AreaCode,
                        subject.PhoneNumber,
                        subject.EmailAddress))
                    .ToListAsync(cancellationToken);

                var types = await _nodeContext.Subject_tbTypes
                    .AsNoTracking()
                    .Select(type => new SubjectTypeRecord(
                        type.SubjectTypeCode,
                        type.SubjectType,
                        type.SubjectClassCode))
                    .ToListAsync(cancellationToken);

                var reals = await _nodeContext.Subject_tbReals
                    .AsNoTracking()
                    .Select(real => new RealRecord(
                        real.SubjectCode,
                        real.FileAs,
                        real.OnMailingList,
                        real.NameTitle,
                        real.NickName,
                        real.JobTitle,
                        real.PhoneNumber,
                        real.MobileNumber,
                        real.EmailAddress,
                        real.Hobby,
                        real.DateOfBirth,
                        real.Department,
                        real.SpouseName,
                        real.HomeNumber,
                        real.Information))
                    .ToListAsync(cancellationToken);

                var virtuals = await _nodeContext.Subject_tbVirtuals
                    .AsNoTracking()
                    .Select(virtualSubject => new VirtualRecord(
                        virtualSubject.SubjectCode,
                        virtualSubject.NumberOfEmployees,
                        virtualSubject.CompanyNumber,
                        virtualSubject.VatNumber,
                        virtualSubject.Eujurisdiction,
                        virtualSubject.BusinessDescription,
                        virtualSubject.Turnover,
                        virtualSubject.WebSite,
                        virtualSubject.SubjectSource))
                    .ToListAsync(cancellationToken);

                var structurals = await _nodeContext.Subject_tbStructurals
                    .AsNoTracking()
                    .Select(structural => new StructuralRecord(
                        structural.SubjectCode,
                        structural.Notes))
                    .ToListAsync(cancellationToken);

                var namespaces = await _nodeContext.Subject_tbNamespaces
                    .AsNoTracking()
                    .Select(ns => new NamespaceRecord(
                        ns.ParentSubjectCode,
                        ns.ChildSubjectCode,
                        ns.Ordinal,
                        ns.IsDefault))
                    .ToListAsync(cancellationToken);

                _snapshot = new SubjectSnapshot(
                    subjects.ToDictionary(subject => subject.SubjectCode, StringComparer.OrdinalIgnoreCase),
                    types.ToDictionary(type => type.SubjectTypeCode),
                    reals.ToDictionary(real => real.SubjectCode, StringComparer.OrdinalIgnoreCase),
                    virtuals.ToDictionary(virtualSubject => virtualSubject.SubjectCode, StringComparer.OrdinalIgnoreCase),
                    structurals.ToDictionary(structural => structural.SubjectCode, StringComparer.OrdinalIgnoreCase),
                    namespaces,
                    namespaces
                        .GroupBy(ns => ns.ParentSubjectCode, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase),
                    namespaces
                        .GroupBy(ns => ns.ChildSubjectCode, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase));

                return _snapshot;
            }
            finally
            {
                _dbGate.Release();
            }
        }

        private static SubjectBrowserNode CreateNode(
            SubjectSnapshot snapshot,
            SubjectRecord subject,
            string namespacePath,
            NamespaceRecord? namespaceRecord)
        {
            var subjectType = snapshot.Types[subject.SubjectTypeCode];
            var subjectClass = (NodeEnum.SubjectClass)subjectType.SubjectClassCode;
            var childCount = snapshot.ChildrenByParent.TryGetValue(subject.SubjectCode, out var children)
                ? children.Count
                : 0;

            return new SubjectBrowserNode {
                SubjectCode = subject.SubjectCode,
                NamespacePath = namespacePath,
                BranchKey = namespacePath,
                Name = subject.SubjectName,
                DisplayLabel = GetDisplayLabel(snapshot, subject.SubjectCode),
                SubjectClass = subjectClass,
                ChildCount = childCount,
                IsDefaultChild = namespaceRecord?.IsDefault ?? false
            };
        }

        private static string GetDisplayLabel(SubjectSnapshot snapshot, string subjectCode)
        {
            if (snapshot.Reals.TryGetValue(subjectCode, out var real) && !string.IsNullOrWhiteSpace(real.FileAs))
            {
                return real.FileAs;
            }

            return snapshot.Subjects[subjectCode].SubjectName;
        }

        private static bool MatchesBranchOrDescendants(
            SubjectSnapshot snapshot,
            string subjectCode,
            string namespacePath,
            string filter,
            IDictionary<string, bool> cache)
        {
            var cacheKey = $"{filter}::{namespacePath}";

            if (cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var subject = snapshot.Subjects[subjectCode];
            var displayLabel = GetDisplayLabel(snapshot, subjectCode);

            if (Contains(namespacePath, filter)
                || Contains(subject.SubjectCode, filter)
                || Contains(subject.SubjectName, filter)
                || Contains(displayLabel, filter))
            {
                cache[cacheKey] = true;
                return true;
            }

            if (snapshot.ChildrenByParent.TryGetValue(subjectCode, out var children))
            {
                foreach (var child in children.OrderBy(child => child.Ordinal).ThenBy(child => child.ChildSubjectCode, StringComparer.OrdinalIgnoreCase))
                {
                    var childPath = $"{namespacePath}.{child.ChildSubjectCode}";

                    if (MatchesBranchOrDescendants(snapshot, child.ChildSubjectCode, childPath, filter, cache))
                    {
                        cache[cacheKey] = true;
                        return true;
                    }
                }
            }

            cache[cacheKey] = false;
            return false;
        }

        private static IReadOnlyList<string> GetNamespacePaths(SubjectSnapshot snapshot, string subjectCode)
        {
            var cache = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
            return ResolvePaths(snapshot, subjectCode, cache);
        }

        private static IReadOnlyList<string> ResolvePaths(
            SubjectSnapshot snapshot,
            string subjectCode,
            IDictionary<string, IReadOnlyList<string>> cache)
        {
            if (cache.TryGetValue(subjectCode, out var cached))
            {
                return cached;
            }

            if (!snapshot.ParentsByChild.TryGetValue(subjectCode, out var parents) || parents.Count == 0)
            {
                var rootPaths = new[] { subjectCode };
                cache[subjectCode] = rootPaths;
                return rootPaths;
            }

            var paths = new List<string>();

            foreach (var parent in parents.OrderBy(parent => parent.Ordinal).ThenBy(parent => parent.ParentSubjectCode, StringComparer.OrdinalIgnoreCase))
            {
                foreach (var parentPath in ResolvePaths(snapshot, parent.ParentSubjectCode, cache))
                {
                    paths.Add($"{parentPath}.{subjectCode}");
                }
            }

            var resolved = paths
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            cache[subjectCode] = resolved;
            return resolved;
        }

        private static IReadOnlyList<SubjectBrowserDetailField> CreateIdentityFields(
            SubjectSnapshot snapshot,
            SubjectRecord subject,
            NodeEnum.SubjectClass subjectClass)
        {
            var fields = new List<SubjectBrowserDetailField>();

            switch (subjectClass)
            {
                case NodeEnum.SubjectClass.Real:
                    if (snapshot.Reals.TryGetValue(subject.SubjectCode, out var real))
                    {
                        AddField(fields, "Title", real.NameTitle);
                        AddField(fields, "Nick Name", real.NickName);
                        AddField(fields, "Role", real.JobTitle);
                        AddField(fields, "Department", real.Department);
                        AddField(fields, "Direct Line", real.PhoneNumber);
                        AddField(fields, "Mobile", real.MobileNumber);
                        AddField(fields, "Email", real.EmailAddress);
                        AddField(fields, "Home Phone", real.HomeNumber);
                        AddField(fields, "Date Of Birth", FormatDate(real.DateOfBirth));
                        AddField(fields, "Hobby", real.Hobby);
                        AddField(fields, "Spouse", real.SpouseName);
                        AddField(fields, "Information", real.Information);
                    }
                    break;

                case NodeEnum.SubjectClass.Virtual:
                    if (snapshot.Virtuals.TryGetValue(subject.SubjectCode, out var virtualSubject))
                    {
                        AddField(fields, "Tax Code", subject.TaxCode);
                        AddField(fields, "Payment Terms", subject.PaymentTerms);
                        AddField(fields, "Expected Days", subject.ExpectedDays.ToString(CultureInfo.InvariantCulture));
                        AddField(fields, "Payment Days", subject.PaymentDays.ToString(CultureInfo.InvariantCulture));
                        AddField(fields, "Days From Month End", FormatBool(subject.PayDaysFromMonthEnd));
                        AddField(fields, "Pay Balance", FormatBool(subject.PayBalance));
                        AddField(fields, "Opening Balance", FormatDecimal(subject.OpeningBalance));
                        AddField(fields, "Area", subject.AreaCode);
                        AddField(fields, "Phone", subject.PhoneNumber);
                        AddField(fields, "Email", subject.EmailAddress);
                        AddField(fields, "Employees", virtualSubject.NumberOfEmployees.ToString(CultureInfo.InvariantCulture));
                        AddField(fields, "Company Number", virtualSubject.CompanyNumber);
                        AddField(fields, "VAT Number", virtualSubject.VatNumber);
                        AddField(fields, "EU Jurisdiction", FormatBool(virtualSubject.Eujurisdiction));
                        AddField(fields, "Turnover", FormatDecimal(virtualSubject.Turnover));
                        AddField(fields, "Web Site", virtualSubject.WebSite);
                        AddField(fields, "Source", virtualSubject.SubjectSource);
                        AddField(fields, "Description", virtualSubject.BusinessDescription);
                    }
                    break;

                case NodeEnum.SubjectClass.Structural:
                    break;
            }

            return fields;
        }

        private static void AddField(ICollection<SubjectBrowserDetailField> fields, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                fields.Add(new SubjectBrowserDetailField(label, value));
            }
        }

        private static string FormatBool(bool value) => value ? "Yes" : "No";

        private static string? FormatDate(DateTime? value)
        {
            return value?.ToString("d", CultureInfo.CurrentCulture);
        }

        private static string FormatDecimal(decimal value)
        {
            return value.ToString("N2", CultureInfo.CurrentCulture);
        }

        private static bool Contains(string? source, string filter)
        {
            return !string.IsNullOrWhiteSpace(source)
                && source.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        private sealed record SubjectRecord(
            string SubjectCode,
            string SubjectName,
            short SubjectTypeCode,
            string? AddressCode,
            string? TaxCode,
            string? PaymentTerms,
            short ExpectedDays,
            short PaymentDays,
            bool PayDaysFromMonthEnd,
            bool PayBalance,
            decimal OpeningBalance,
            string? AreaCode,
            string? PhoneNumber,
            string? EmailAddress);

        private sealed record SubjectTypeRecord(
            short SubjectTypeCode,
            string SubjectType,
            short SubjectClassCode);

        private sealed record RealRecord(
            string SubjectCode,
            string? FileAs,
            bool OnMailingList,
            string? NameTitle,
            string? NickName,
            string? JobTitle,
            string? PhoneNumber,
            string? MobileNumber,
            string? EmailAddress,
            string? Hobby,
            DateTime? DateOfBirth,
            string? Department,
            string? SpouseName,
            string? HomeNumber,
            string? Information);

        private sealed record VirtualRecord(
            string SubjectCode,
            int NumberOfEmployees,
            string? CompanyNumber,
            string? VatNumber,
            bool Eujurisdiction,
            string? BusinessDescription,
            decimal Turnover,
            string? WebSite,
            string? SubjectSource);

        private sealed record StructuralRecord(
            string SubjectCode,
            string? Notes);

        private sealed record NamespaceRecord(
            string ParentSubjectCode,
            string ChildSubjectCode,
            int Ordinal,
            bool IsDefault);

        private sealed record SubjectSnapshot(
            IDictionary<string, SubjectRecord> Subjects,
            IDictionary<short, SubjectTypeRecord> Types,
            IDictionary<string, RealRecord> Reals,
            IDictionary<string, VirtualRecord> Virtuals,
            IDictionary<string, StructuralRecord> Structurals,
            IReadOnlyList<NamespaceRecord> Namespaces,
            IDictionary<string, List<NamespaceRecord>> ChildrenByParent,
            IDictionary<string, List<NamespaceRecord>> ParentsByChild);
    }
}
