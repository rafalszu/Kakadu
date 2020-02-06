using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Configuration;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Kakadu.ActionApi.Clients
{
    public class AuthenticationClient : ClientBase, IAuthenticationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationClient> _logger;

        private readonly IOptions<ApiConfiguration> _options;

        public AuthenticationClient(HttpClient httpClient, ILogger<AuthenticationClient> logger, IOptions<ApiConfiguration> options) : base(httpClient, logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _httpClient.BaseAddress = new Uri(options.Value.Address);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<string> RequestTokenAsync()
        {
            // var tokenRequest = new JwtTokenDTO
            // {
            //     Username = _options.Value.Username,
            //     Password = _options.Value.Password
            // };

            // using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            // {
            //     var dto = await this.PostAsync<UserDTO>(tokenRequest, "token/authenticate", cancellationTokenSource.Token);
            //     return dto?.Token;
            // }
            throw new NotImplementedException();
        }
    }
}