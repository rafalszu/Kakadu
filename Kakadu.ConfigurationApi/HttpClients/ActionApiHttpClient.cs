using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.Common.HttpClients;
using Kakadu.ConfigurationApi.Interfaces;
using Kakadu.DTO;
using Microsoft.Extensions.Logging;

namespace Kakadu.ConfigurationApi.HttpClients
{
    public class ActionApiHttpClient : HttpClientBase, IActionApiHttpClient
    {
        private readonly ILogger<ActionApiHttpClient> _logger;
        public ActionApiHttpClient(HttpClient httpClient, ILogger<ActionApiHttpClient> logger) : base(httpClient, logger) => _logger = logger;

        public async Task<bool> StartRecordingAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken)
        {
            return await QueryActionInstanceAsync<bool>("start", host, serviceCode, accessToken, cancellationToken);
        }

        public async Task<bool> StopRecordingAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken)
        {
            return await QueryActionInstanceAsync<bool>("stop", host, serviceCode, accessToken, cancellationToken);
        }

        public async Task<bool> GetStatusAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken)
        {
            return await QueryActionInstanceAsync<bool>("status", host, serviceCode, accessToken, cancellationToken);
        }

        public async Task<List<ServiceCaptureStatusDTO>> GetStatusesAsync(string host, string accessToken, CancellationToken cancellationToken)
        {
            return await QueryActionInstanceAsync<List<ServiceCaptureStatusDTO>>("status", host, null, accessToken, cancellationToken);
        }

        private async Task<T> QueryActionInstanceAsync<T>(string apiMethod, string host, string serviceCode, string accessToken, CancellationToken cancellationToken)
        {
            var uri = GetUri(apiMethod, host, serviceCode);

            this.CustomRequestHeaders = new Dictionary<string, IEnumerable<string>> {
                { "Authorization", new List<string> { accessToken } }
            };

            return await this.GetAsync<T>(uri, cancellationToken);
        }

        private Uri GetUri(string apiMethod, string host, string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(apiMethod))
                throw new ArgumentNullException(nameof(apiMethod));
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentNullException(nameof(host));

            return new Uri(string.Format("{0}{1}api/v1/record/{2}{3}{4}", 
                host,
                host.EndsWith('/') ? string.Empty : "/",
                apiMethod,
                !string.IsNullOrWhiteSpace(serviceCode) ? "/" : string.Empty,
                serviceCode), UriKind.Absolute);
        }
    }
}