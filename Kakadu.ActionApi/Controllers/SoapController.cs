using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Kakadu.ActionApi.Extensions;
using Kakadu.ActionApi.Interfaces;
using Kakadu.DTO;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;

namespace Kakadu.ActionApi.Controllers
{
    [ApiController]
    [Consumes("text/xml", "application/soap+xml", "application/soap", "application/xml", "text/xml")]
    [Route("soap/{serviceCode}")]
    public class SoapController : BaseActionApiController
    {
        private readonly ILogger<SoapController> _logger;

        public SoapController(ILogger<SoapController> logger, IAnonymousServiceHttpClient anonymousServiceClient, IAuthenticatedServiceHttpClient authenticatedServiceClient, IDistributedCache cache) 
            : base(logger, anonymousServiceClient, authenticatedServiceClient, cache) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [Route("{**catchAll}")]
        [HttpPost]
        public async Task ProcessRequest(string serviceCode, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            object routeValues = null;
            if(Request.RouteValues.ContainsKey("catchAll")) {
                routeValues = Request.RouteValues["catchAll"];
            }

            var relativePath = $"/{routeValues ?? string.Empty}{(Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty)}";

            Request.EnableBuffering(); 

            await ProxyCall(serviceCode, relativePath, cancellationToken).ConfigureAwait(false);
        }

        public override KnownRouteDTO GetKnownRoute(ServiceDTO service, string relativePath, string action)
        {
            if(service == null)
                throw new ArgumentNullException(nameof(service));
            if(string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentNullException(nameof(relativePath));

            var knownRoutes = service.KnownRoutes?.Where(r => !string.IsNullOrWhiteSpace(r.Action) && 
                                                            r.RelativeUrl.Equals(relativePath, StringComparison.InvariantCultureIgnoreCase));
                                                            
            if(knownRoutes != null && knownRoutes.Any() && !string.IsNullOrWhiteSpace(action)) {
                // match action
                var actionRoute = knownRoutes.FirstOrDefault(r => r.Action.Equals(action, StringComparison.InvariantCultureIgnoreCase));
                return actionRoute;
            }

            return null;
        }
    }
}