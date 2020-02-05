using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;

namespace Kakadu.ActionApi.Handlers
{
    public class ApiBearerTokenHandler : DelegatingHandler
    {
        private readonly IAuthenticationClient _authenticationClient;
        public ApiBearerTokenHandler(IAuthenticationClient authenticationClient)
        {
            _authenticationClient = authenticationClient ?? throw new ArgumentNullException(nameof(authenticationClient));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // request the access token
            var accessToken = await _authenticationClient.RequestTokenAsync();

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            return await base.SendAsync(request, cancellationToken);
        }
    }
}