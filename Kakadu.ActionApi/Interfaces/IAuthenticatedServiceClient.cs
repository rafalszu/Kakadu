using System.Threading.Tasks;
using Kakadu.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Kakadu.ActionApi.Interfaces
{
    public interface IAuthenticatedServiceClient : IAnonymousServiceHttpClient
    {
        Task StoreReply(KnownRouteDTO dto);
    }
}