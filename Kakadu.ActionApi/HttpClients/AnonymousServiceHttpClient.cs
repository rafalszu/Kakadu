using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.Common.HttpClients;
using Kakadu.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Kakadu.Common.Extensions;
using Kakadu.DTO.Constants;

namespace Kakadu.ActionApi.HttpClients
{
    public class AnonymousServiceHttpClient : HttpClientBase, IAnonymousServiceHttpClient
    {
        private readonly IDistributedCache _cache;

        public AnonymousServiceHttpClient(HttpClient httpClient, ILogger<AnonymousServiceHttpClient> logger, IDistributedCache cache) : base(httpClient, logger) 
            => _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        public async Task<ServiceDTO> GetByCodeAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var recordCacheKey = KakaduConstants.GetRecordKey(serviceCode);
            var isRecording = (await _cache.GetAsync<bool?>(recordCacheKey, cancellationToken)) ?? false;
            
            var dto = await _cache.GetOrAddAsync<ServiceDTO>(serviceCode, async (options) => {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);

                var result = await this.GetAsync<ServiceDTO>($"service/{serviceCode}", cancellationToken);

                result.IsRecording = isRecording;
                
                return result;
            }, token: cancellationToken);
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