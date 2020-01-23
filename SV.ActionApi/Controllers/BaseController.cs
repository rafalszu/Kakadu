using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SV.ActionApi.Extensions;
using SV.Core.Interfaces;
using SV.Core.Models;

namespace SV.ActionApi.Controllers
{
    public class BaseController : ControllerBase
    {
        private ServiceModel serviceModel;
        private string relativePath;
        private readonly IRepository repository;

        public BaseController(IRepository repository)
        {
            this.repository = repository;
        }

        [NonAction]
        public Task ProxyCall(ServiceModel serviceModel, string relativePath, 
        Func<HttpContext, Task<bool>> interceptKnownRouteAsync, ILogger logger, bool saveResponse = false)
        {
            if(serviceModel == null)
                throw new ArgumentNullException(nameof(serviceModel));
            if(relativePath == null)
                throw new ArgumentNullException(nameof(relativePath));
            if(logger == null)
                throw new ArgumentNullException(nameof(logger));

            this.serviceModel = serviceModel;
            this.relativePath = relativePath;

            var options = ProxyOptions.Instance
                .WithHttpClientName("ServiceVirtualizationClient")
                .WithShouldAddForwardedHeaders(true)
                .WithIntercept(async (context) => await interceptKnownRouteAsync(context))                
                //.WithBeforeSend
                .WithAfterReceive((context, response) => {
                    if(saveResponse) {
                        logger.LogInformation($"Saving response from {relativePath}");

                        // TODO: try match already existing knownroute and update it just-received data
                        (serviceModel.KnownRoutes ??= new List<KnownRouteModel>()).Add(response.ToKnownRoute());
                        repository.UpdateService(serviceModel);

                        logger.LogInformation($"{serviceModel.Code} known routes updated");
                    }

                    return Task.CompletedTask;
                });

            var url = CombinePaths(serviceModel.Address.AbsoluteUri, relativePath);

            return this.ProxyAsync(url, options);
        }

        internal string CombinePaths(string s, string v)
        {
            if(string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(v))
                throw new Exception("Can't combine paths as at least one part is null or empty");

            return string.Format("{0}/{1}", s.TrimEnd('/'), v.TrimStart('/'));
        }

        internal async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));
            
            if (request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase) ||
                request.Method.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase) ||
                request.Method.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase))
                return string.Empty;

            string body = string.Empty;

            // Allows using several time the stream in ASP.Net Core
            Request.EnableBuffering(); 

            using (StreamReader reader  = new StreamReader(
                stream: HttpContext.Request.Body,
                encoding: System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024, 
                leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            HttpContext.Request.Body.Position = 0;

            return body;
        }
    }
}