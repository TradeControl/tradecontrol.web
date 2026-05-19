using System.Threading;
using System.Threading.Tasks;

namespace TradeControl.Web.AppServices
{
    public interface ICashNamespaceResolver
    {
        Task<string> ResolveNamespacePathAsync(
            string subjectCode,
            string? parentSubjectCode,
            CancellationToken cancellationToken = default);
    }
}
