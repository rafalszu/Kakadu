using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;

namespace Kakadu.ActionApi.Controllers
{
    [ApiController]
    [Consumes("application/json")]
    [Route("rest/{serviceCode}")]
    public class RestController : BaseActionApiController
    {
        private readonly ILogger<RestController> _logger;

        public RestController(ILogger<RestController> logger, IAnonymousServiceHttpClient anonymousServiceClient, IAuthenticatedServiceHttpClient authenticatedServiceClient, IDistributedCache cache) 
            : base(logger, anonymousServiceClient, authenticatedServiceClient, cache) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpPatch]
        [HttpDelete]
        [Route("{**catchAll}")]
        public async Task ProcessRequest(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            object routeValues = null;
            if(Request.RouteValues.ContainsKey("catchAll")) {
                routeValues = Request.RouteValues["catchAll"];
            }

            string relativePath = $"/{routeValues ?? string.Empty}{(Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty)}";

            await ProxyCall(serviceCode, relativePath, cancellationToken).ConfigureAwait(false);
        }

        public override KnownRouteDTO GetKnownRoute(ServiceDTO service, string relativePath, string action)
        {
            if(service == null)
                throw new ArgumentNullException(nameof(service));
            if(string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentNullException(nameof(relativePath));

            return service.KnownRoutes?.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.RelativeUrl) && r.RelativeUrl.Equals(relativePath, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}