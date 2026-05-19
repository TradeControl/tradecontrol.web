using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Data;

namespace TradeControl.Web.AppServices
{
    public sealed class CashNamespaceResolver : ICashNamespaceResolver
    {
        private readonly NodeContext _nodeContext;
        private Dictionary<string, IReadOnlyList<NamespaceEdge>>? _parentsByChild;

        public CashNamespaceResolver(NodeContext nodeContext)
        {
            _nodeContext = nodeContext;
        }

        public async Task<string> ResolveNamespacePathAsync(
            string subjectCode,
            string? parentSubjectCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(subjectCode))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(parentSubjectCode))
            {
                return subjectCode;
            }

            await EnsureLoadedAsync(cancellationToken);

            var segments = new List<string> { subjectCode };
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { subjectCode };
            var currentParentSubjectCode = parentSubjectCode;

            while (!string.IsNullOrWhiteSpace(currentParentSubjectCode))
            {
                if (!visited.Add(currentParentSubjectCode))
                {
                    break;
                }

                segments.Insert(0, currentParentSubjectCode);

                if (_parentsByChild is null
                    || !_parentsByChild.TryGetValue(currentParentSubjectCode, out var parents)
                    || parents.Count == 0)
                {
                    break;
                }

                currentParentSubjectCode = parents[0].ParentSubjectCode;
            }

            return string.Join('.', segments);
        }

        private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
        {
            if (_parentsByChild is not null)
            {
                return;
            }

            var rows = await _nodeContext.Subject_tbNamespaces
                .AsNoTracking()
                .OrderByDescending(row => row.IsDefault)
                .ThenBy(row => row.Ordinal)
                .ThenBy(row => row.ParentSubjectCode)
                .Select(row => new NamespaceEdge(
                    row.ParentSubjectCode,
                    row.ChildSubjectCode,
                    row.IsDefault,
                    row.Ordinal))
                .ToListAsync(cancellationToken);

            _parentsByChild = rows
                .GroupBy(row => row.ChildSubjectCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<NamespaceEdge>)group
                        .OrderByDescending(row => row.IsDefault)
                        .ThenBy(row => row.Ordinal)
                        .ThenBy(row => row.ParentSubjectCode, StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);
        }

        private sealed record NamespaceEdge(
            string ParentSubjectCode,
            string ChildSubjectCode,
            bool IsDefault,
            int Ordinal);
    }
}
