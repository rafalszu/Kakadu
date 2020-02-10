using System.Threading;
using System.Threading.Tasks;
using Kakadu.DTO;

namespace Kakadu.ActionApi.Interfaces
{
    public interface IAnonymousServiceHttpClient
    {
        Task<ServiceDTO> GetByCodeAsync(string serviceCode, CancellationToken cancellationToken);


        Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken);
    }
}