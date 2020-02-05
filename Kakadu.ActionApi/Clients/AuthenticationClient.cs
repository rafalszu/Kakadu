using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kakadu.ActionApi.Configuration;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Kakadu.ActionApi.Clients
{
    public class AuthenticationClient : IAuthenticationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationClient> _logger;

        private readonly IOptions<ApiConfiguration> _options;

        public AuthenticationClient(HttpClient httpClient, ILogger<AuthenticationClient> logger, IOptions<ApiConfiguration> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _httpClient.BaseAddress = new Uri(options.Value.Address);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<string> RequestTokenAsync()
        {
            string payload = JsonConvert.SerializeObject(new JwtTokenDTO
            {
                Username = _options.Value.Username,
                Password = _options.Value.Password
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            UserDTO dto = null;

            using(var response = await _httpClient.PostAsync("token", content))
            {
                if(!response.IsSuccessStatusCode)
                {
                    string msg = $"Unable to get bearer token for user '{_options.Value.Username}': {response.StatusCode}";
                    _logger.LogError(msg);
                    throw new Exception(msg);
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    dto = JsonConvert.DeserializeObject<UserDTO>(responseContent);
                }
                catch(Exception)
                {
                    throw new Exception("Unable to deserialize response content");
                }
            }

            return dto?.Token;
        }
    }
}