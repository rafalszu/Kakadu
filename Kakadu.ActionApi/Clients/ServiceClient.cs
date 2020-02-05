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
    public class ServiceClient : ClientBase, IServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAppCache _cache;

        public ServiceClient(HttpClient httpClient, ILogger<ServiceClient> logger, IAppCache cache) : base(httpClient, logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

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