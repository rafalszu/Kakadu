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

namespace Kakadu.ActionApi.Controllers
{
    [ApiController]
    [Consumes("application/json")]
    [Route("rest/{serviceCode}")]
    public class RestController : BaseActionApiController
    {
        private readonly ILogger<RestController> _logger;

        public RestController(ILogger<RestController> logger, IServiceClient serviceClient) : base(logger, serviceClient) => _logger = logger;

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

            string relativePath = string.Format("/{0}{1}", routeValues ?? string.Empty, Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty);

            await ProxyCall(serviceCode, relativePath, cancellationToken).ConfigureAwait(false);
        }

        public override KnownRouteDTO GetKnownRoute(ServiceDTO service, string relativePath, string action)
        {
            if(service == null)
                throw new ArgumentNullException(nameof(service));

            return service.KnownRoutes?.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.RelativeUrl) && r.RelativeUrl.Equals(relativePath, StringComparison.InvariantCultureIgnoreCase));
        }
    }
    
    
    //     private async Task<bool> InterceptKnownRouteAsync(HttpContext context)
    //     {
    //         var knownRoute = service.KnownRoutes?.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.RelativeUrl) && r.RelativeUrl.Equals(relativeUrl, StringComparison.InvariantCultureIgnoreCase));
    //         if(knownRoute != null)
    //         {
    //             _logger.LogInformation($"Found known route for '{relativeUrl}', intercepting http call");

    //             var knownResponse = knownRoute.Replies?.FirstOrDefault();
    //             if(knownResponse != null) 
    //             {
    //                 _logger.LogInformation($"Found matching response for '{relativeUrl}' request");

    //                 if(knownResponse.Headers?.Any() ?? false)
    //                 {
    //                     foreach(var headerKey in knownResponse.Headers.Keys)
    //                         context.Response.Headers[headerKey] = knownResponse.Headers[headerKey];
    //                 }

    //                 context.Response.StatusCode = knownResponse.StatusCode;
    //                 context.Response.ContentType = knownResponse.ContentTypeString;
    //                 context.Response.ContentLength = knownResponse.ContentLength;
    //                 if(!string.IsNullOrWhiteSpace(knownResponse.ContentEncoding))
    //                     context.Response.Headers["Content-Encoding"] = knownResponse.ContentEncoding;

    //                 await context.Response.Body.WriteAsync(knownResponse.ContentRaw, 0, knownResponse.ContentRaw.Length);

    //                 return true;
    //             }
                
    //             // no replies stored, can passthrough the call?
    //             if(service.UnkownRoutesPassthrough)
    //             {
    //                 _logger.LogInformation($"No responses found for '{relativeUrl}', passing http call through");
    //                 return false;
    //             }

    //             // intercept call, return 200 with no content
    //             _logger.LogInformation($"No respones found for '{relativeUrl}', can't pass through as configured; returning NoContent");
    //             context.Response.StatusCode = 200;
    //             return true;
    //         }
            
    //         // route not known, but dont intercept and play ball
    //         if(service.UnkownRoutesPassthrough)
    //         {
    //             _logger.LogInformation($"No known routes found for '{relativeUrl}', passign http call through");
    //             return false;
    //         }

    //         // route not known, no passthrough, intercept with 404
    //         _logger.LogInformation($"No known routes found for '{relativeUrl}', can't pass through as configured; returning NotFound");
    //         context.Response.StatusCode = 404;
    //         return true;
    //     }
    // }
}