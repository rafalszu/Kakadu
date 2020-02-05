using System.Threading.Tasks;
using Kakadu.DTO;

namespace Kakadu.ActionApi.Interfaces
{
    public interface IServiceClient
    {
         Task<ServiceDTO> GetByCodeAsync(string serviceCode);
    }
}