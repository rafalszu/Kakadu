using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using Kakadu.DTO.Constants;
using LazyCache;
using Microsoft.Extensions.Logging;

namespace Kakadu.ActionApi.Clients
{
    public class AuthenticatedServiceClient : AnonymousServiceClient, IAuthenticatedServiceClient
    {
        public AuthenticatedServiceClient(HttpClient client, ILogger<AuthenticatedServiceClient> logger, IAppCache cache) : base(client, logger, cache)
        {
            string accessToken = cache.Get<string>(KakaduConstants.ACCESS_TOKEN);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}