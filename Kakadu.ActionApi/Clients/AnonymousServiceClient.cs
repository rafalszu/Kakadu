using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using LazyCache;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kakadu.ActionApi.Clients
{
    public class AnonymousServiceClient : ClientBase, IAnonymousServiceClient
    {
        private readonly IAppCache _cache;

        public AnonymousServiceClient(HttpClient httpClient, ILogger<AnonymousServiceClient> logger, IAppCache cache) : base(httpClient, logger) => _cache = cache;

        public async Task<ServiceDTO> GetByCodeAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var dto = await _cache.GetOrAddAsync(serviceCode, async entry => {
                return await this.GetAsync<ServiceDTO>($"service/{serviceCode}", cancellationToken);
            });
            return dto;
        }
    }
}