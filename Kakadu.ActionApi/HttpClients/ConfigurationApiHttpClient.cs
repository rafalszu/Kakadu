using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Kakadu.Common.HttpClients;
using LazyCache;
using Microsoft.Extensions.Logging;

namespace Kakadu.ActionApi.HttpClients
{
    public class ConfigurationApiHttpClient : HttpClientBase, IConfigurationApiHttpClient
    {
        public ConfigurationApiHttpClient(HttpClient client, ILogger<HttpClientBase> logger, IAppCache cache) : base(client, logger)
        {
        }

        public async Task Register(string url, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            
            await this.PostAsync<string>(url, "actionapi/register", cancellationToken).ConfigureAwait(false);
        }

        public async Task Unregister(string url, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            await this.PostAsync<string>(url, "actionapi/unregister", cancellationToken).ConfigureAwait(false);
        }
    }
}