using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using Newtonsoft.Json;

namespace Kakadu.ActionApi.Clients
{
    public class ServiceClient : ClientBase, IServiceClient
    {
        private readonly HttpClient _httpClient;

        public ServiceClient(HttpClient httpClient) : base(httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ServiceDTO> GetByCodeAsync(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            var response = await this.GetAsync(new Uri($"service/{serviceCode}", UriKind.Relative));
            if(TryDeserialize<ServiceDTO>(response, out ServiceDTO dto))
                return dto;

            return null;
        }
    }
}