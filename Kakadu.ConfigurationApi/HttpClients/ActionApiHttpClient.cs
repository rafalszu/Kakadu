using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.Common.HttpClients;
using Kakadu.ConfigurationApi.Interfaces;
using Microsoft.Extensions.Logging;

namespace Kakadu.ConfigurationApi.HttpClients
{
    public class ActionApiHttpClient : HttpClientBase, IActionApiHttpClient
    {
        private readonly ILogger<ActionApiHttpClient> _logger;
        public ActionApiHttpClient(HttpClient httpClient, ILogger<ActionApiHttpClient> logger) : base(httpClient, logger) => _logger = logger;

        public async Task<bool> StartRecordingAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken)
        {
            return await ToggleRecord("start", host, serviceCode, accessToken, cancellationToken);
        }

        public async Task<bool> StopRecordingAsync(string host, string serviceCode, string accessToken, CancellationToken cancellationToken)
        {
            return await ToggleRecord("stop", host, serviceCode, accessToken, cancellationToken);
        }

        private async Task<bool> ToggleRecord(string apiMethod, string host, string serviceCode, string accessToken, CancellationToken cancellationToken)
        {
            var uri = new Uri(string.Format("{0}{1}api/v1/record/{2}/{3}", 
                host,
                host.EndsWith('/') ? string.Empty : "/",
                apiMethod,
                serviceCode), UriKind.Absolute);

            this.CustomRequestHeaders = new Dictionary<string, IEnumerable<string>> {
                { "Authorization", new List<string> { accessToken } }
            };

            return await this.GetAsync<bool>(uri, cancellationToken);
        }
    }
}