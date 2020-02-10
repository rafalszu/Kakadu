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

namespace Kakadu.ActionApi.HttpClients
{
    public class AuthenticatedServiceHttpClient : AnonymousServiceHttpClient, IAuthenticatedServiceHttpClient
    {
        private readonly ILogger<AuthenticatedServiceHttpClient> _logger;

        public AuthenticatedServiceHttpClient(HttpClient client, ILogger<AuthenticatedServiceHttpClient> logger, IAppCache cache) : base(client, logger, cache)
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