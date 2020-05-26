using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.DTO;

namespace Kakadu.ConfigurationApi.Interfaces
{
    public interface IActionApiService
    {
        Task<IEnumerable<ServiceCaptureResultDTO>> GetStatusesAsync(CancellationToken cancellationToken = default);

        Task<List<ActionApiCallResultDTO>> GetStatusAsync(string serviceCode, CancellationToken cancellationToken = default);

        Task<List<ActionApiCallResultDTO>> StartRecordingAsync(string serviceCode, CancellationToken cancellationToken = default);

        Task<List<ActionApiCallResultDTO>> StopRecordingAsync(string serviceCode, CancellationToken cancellationToken = default);
    }
}