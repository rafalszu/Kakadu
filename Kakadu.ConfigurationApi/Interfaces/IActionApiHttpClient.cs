using System.Threading;
using System.Threading.Tasks;

namespace Kakadu.ConfigurationApi.Interfaces
{
    public interface IActionApiHttpClient
    {
        Task<bool> StartRecordingAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken);

        Task<bool> StopRecordingAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken);

        Task<bool> GetStatusAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken);
    }
}