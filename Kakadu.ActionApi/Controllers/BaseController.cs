using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Kakadu.ActionApi.Extensions;

[assembly: InternalsVisibleTo("Kakadu.ActionApi.Tests")]
namespace Kakadu.ActionApi.Controllers
{
    // public class BaseController : ControllerBase
    // {
    //     private ServiceModel serviceModel;
    //     private string relativePath;
    //     private readonly IDataProvider dataProvider;

    //     public BaseController(IDataProvider dataProvider)
    //     {
    //         this.dataProvider = dataProvider;
    //     }

    //     [NonAction]
    //     public Task ProxyCall(ServiceModel serviceModel, string relativePath, 
    //     Func<HttpContext, Task<bool>> interceptKnownRouteAsync, ILogger logger, bool saveResponse = false)
    //     {
    //         if(serviceModel == null)
    //             throw new ArgumentNullException(nameof(serviceModel));
    //         if(relativePath == null)
    //             throw new ArgumentNullException(nameof(relativePath));
    //         if (interceptKnownRouteAsync == null)
    //             throw new ArgumentNullException(nameof(interceptKnownRouteAsync));
    //         if(logger == null)
    //             throw new ArgumentNullException(nameof(logger));

    //         this.serviceModel = serviceModel;
    //         this.relativePath = relativePath;

    //         var options = ProxyOptions.Instance
    //             .WithHttpClientName("ServiceVirtualizationClient")
    //             .WithShouldAddForwardedHeaders(true)
    //             .WithIntercept(async (context) => await interceptKnownRouteAsync(context))                
    //             //.WithBeforeSend
    //             .WithAfterReceive((context, response) => {
    //                 if(saveResponse) {
    //                     logger.LogInformation($"Saving response from {relativePath}");

    //                     // TODO: try match already existing knownroute and update it just-received data
    //                     (serviceModel.KnownRoutes ??= new List<KnownRouteModel>()).Add(response.ToKnownRoute());
    //                     dataProvider.UpdateService(serviceModel);

    //                     logger.LogInformation($"{serviceModel.Code} known routes updated");
    //                 }

    //                 return Task.CompletedTask;
    //             });

    //         var url = CombinePaths(serviceModel.Address.AbsoluteUri, relativePath);

    //         return this.ProxyAsync(url, options);
    //     }

    //     internal string CombinePaths(string s, string v)
    //     {
    //         if(string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(v))
    //             throw new Exception("Can't combine paths as at least one part is null or empty");

    //         return string.Format("{0}/{1}", s.TrimEnd('/'), v.TrimStart('/'));
    //     }

        
    //     internal async Task<string> GetRequestBodyAsync(string httpRequestMethod, Stream httpRequestBody)
    //     {
    //         if(string.IsNullOrWhiteSpace(httpRequestMethod))
    //             throw new ArgumentNullException(nameof(httpRequestMethod));
    //         if(httpRequestBody == null)
    //             throw new ArgumentNullException(nameof(httpRequestBody));
            
    //         if (httpRequestMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase) ||
    //             httpRequestMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase) ||
    //             httpRequestMethod.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase))
    //             return string.Empty;

    //         string body = string.Empty;

    //         using (StreamReader reader  = new StreamReader(
    //             stream: httpRequestBody,
    //             encoding: System.Text.Encoding.UTF8,
    //             detectEncodingFromByteOrderMarks: true,
    //             bufferSize: 1024, 
    //             leaveOpen: true))
    //         {
    //             body = await reader.ReadToEndAsync();
    //         }

    //         // Rewind, so the core is not lost when it looks the body for the request
    //         httpRequestBody.Position = 0;

    //         return body;
    //     }
    // }
}