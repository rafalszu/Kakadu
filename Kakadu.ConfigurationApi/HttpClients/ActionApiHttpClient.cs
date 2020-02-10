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

        public Task<bool> StartRecording(string url, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }
}