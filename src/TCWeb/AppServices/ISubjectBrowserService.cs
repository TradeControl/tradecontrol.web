using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeControl.Web.Pages.Subject.Browser;
using TradeControl.Web.Pages.Subject.Controls;

namespace TradeControl.Web.AppServices
{
    public sealed class SubjectBrowserPageResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
        public bool HasMorePages { get; init; }
    }

    public interface ISubjectBrowserService
    {
        Task<SubjectBrowserPageResult<SubjectBrowserNode>> GetRootNodesAsync(
            string namespaceFilter,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<SubjectBrowserPageResult<SubjectBrowserNode>> GetChildNodesAsync(
            string parentSubjectCode,
            string parentNamespacePath,
            string namespaceFilter,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NamespaceSelectorSuggestion>> GetNamespaceSuggestionsAsync(
            string filterText,
            int maxResults,
            CancellationToken cancellationToken = default);

        Task<SubjectBrowserDetailModel?> GetDetailAsync(
            string subjectCode,
            string? parentSubjectCode,
            CancellationToken cancellationToken = default);

        void InvalidateSnapshot();
    }
}
