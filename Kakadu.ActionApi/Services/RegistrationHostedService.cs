using System;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kakadu.ActionApi.Services
{
    public class RegistrationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        public RegistrationHostedService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var service = _serviceProvider.GetRequiredService<IActionApiClient>();
            string url = _configuration[WebHostDefaults.ServerUrlsKey];
            
            await service.Register(url, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var service = _serviceProvider.GetRequiredService<IActionApiClient>();

            string url = _configuration[WebHostDefaults.ServerUrlsKey];
            await service.Unregister(url, cancellationToken);
        }
    }
}