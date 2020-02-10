using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using Kakadu.DTO.Constants;
using Kakadu.DTO.HttpExceptions;
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
            // bearer xxxxxxxx
            if(string.IsNullOrWhiteSpace(accessToken))
                throw new HttpUnauthorizedException("unauthorized");

            var parts = accessToken.Split(' ');
            if(parts.Length == 1)
                throw new Exception("Wrong bearer token format");
            
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(parts[0], parts[1]);
        }

        public async Task StoreReply(string serviceCode, KnownRouteDTO dto, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));
            if(dto == null)
                throw new ArgumentNullException(nameof(dto));
                
            // StoreFoundRouteInCache
            await this.PostAsync<string>(dto, $"knownroute/store/{serviceCode}", cancellationToken);

            _logger.LogInformation("dto sent to configuration api");
        }
    }
}