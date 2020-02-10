using System.Threading;
using System.Threading.Tasks;

namespace Kakadu.ActionApi.Interfaces
{
    public interface IConfigurationApiHttpClient
    {
        Task Register(string url, CancellationToken cancellationToken);

        Task Unregister(string url, CancellationToken cancellationToken);
    }
}