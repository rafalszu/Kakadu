using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kakadu.ActionApi.Clients
{
    public class ServiceClient : ClientBase, IServiceClient
    {
        private readonly HttpClient _httpClient;

        public ServiceClient(HttpClient httpClient, ILogger<ServiceClient> logger) : base(httpClient, logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ServiceDTO> GetByCodeAsync(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var dto = await this.GetAsync<ServiceDTO>($"service/{serviceCode}", cancellationToken);
            return dto;
        }
    }
}