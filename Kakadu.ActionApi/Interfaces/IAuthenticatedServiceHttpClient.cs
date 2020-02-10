using System.Threading.Tasks;
using Kakadu.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Kakadu.ActionApi.Interfaces
{
    public interface IAuthenticatedServiceHttpClient : IAnonymousServiceHttpClient
    {
        Task StoreReply(KnownRouteDTO dto);
    }
}