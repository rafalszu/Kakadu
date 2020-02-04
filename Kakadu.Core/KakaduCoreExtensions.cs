using Kakadu.Core.Interfaces;
using Kakadu.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kakadu.Core
{
    public static class KakaduCoreExtensions
    {
        public static void RegisterKakaduServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IServiceService, ServiceService>();
        }
    }
}