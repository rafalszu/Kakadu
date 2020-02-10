using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Kakadu.ActionApi.Interfaces
{
    public interface IAuthenticatedServiceClient : IAnonymousServiceClient
    {
        Task StoreRecoredReplies();
    }
}