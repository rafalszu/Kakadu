using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using Kakadu.DTO.Constants;
using LazyCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kakadu.ActionApi.Clients
{
    public class AuthenticatedServiceClient : AnonymousServiceHttpClient, IAuthenticatedServiceClient
    {
        private readonly ILogger<AuthenticatedServiceClient> _logger;

        public AuthenticatedServiceClient(HttpClient client, ILogger<AuthenticatedServiceClient> logger, IAppCache cache) : base(client, logger, cache)
        {
            _logger = logger;

            string accessToken = cache.Get<string>(KakaduConstants.ACCESS_TOKEN);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task StoreReply(KnownRouteDTO dto)
        {
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));
                
            _logger.LogInformation("dto sent to configuration api");
            // TODO: send recorded replies to config api
        }
    }
}