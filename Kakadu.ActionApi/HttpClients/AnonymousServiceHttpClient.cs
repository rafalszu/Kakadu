using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.Common.HttpClients;
using Kakadu.DTO;
using LazyCache;
using Microsoft.Extensions.Logging;

namespace Kakadu.ActionApi.HttpClients
{
    public class AnonymousServiceHttpClient : HttpClientBase, IAnonymousServiceHttpClient
    {
        private readonly IAppCache _cache;

        public AnonymousServiceHttpClient(HttpClient httpClient, ILogger<AnonymousServiceHttpClient> logger, IAppCache cache) : base(httpClient, logger) => _cache = cache;

        public async Task<ServiceDTO> GetByCodeAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var dto = await _cache.GetOrAddAsync(serviceCode, async entry => {
                return await this.GetAsync<ServiceDTO>($"service/{serviceCode}", cancellationToken);
            });
            return dto;
        }

        public async Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(accessToken))
                return false;

            this.CustomRequestHeaders = new Dictionary<string, IEnumerable<string>> {
                { 
                    "Authorization", new List<string> { accessToken } 
                }
            };

            var result = await this.PostAsync<bool>(null, "token/validate", cancellationToken);
            return result;
        }
    }
}