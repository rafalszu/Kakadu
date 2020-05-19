using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.DTO;

namespace Kakadu.ConfigurationApi.Interfaces
{
    public interface IActionApiHttpClient
    {
        Task<bool> StartRecordingAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken);

        Task<bool> StopRecordingAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken);

        Task<bool> GetStatusAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken);

        Task<List<ServiceCaptureStatusDTO>> GetStatusesAsync(string host, string accessToken, CancellationToken cancellationToken);
    }
}