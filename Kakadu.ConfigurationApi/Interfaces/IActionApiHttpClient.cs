using System.Threading;
using System.Threading.Tasks;

namespace Kakadu.ConfigurationApi.Interfaces
{
    public interface IActionApiHttpClient
    {
        Task<bool> StartRecording(string url, CancellationToken cancellationToken);
    }
}