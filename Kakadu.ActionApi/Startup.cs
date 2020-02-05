using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AspNetCore.Proxy;
using Kakadu.ActionApi.Configuration;
using Kakadu.ActionApi.Interfaces;
using Kakadu.ActionApi.Clients;
using Kakadu.ActionApi.Handlers;
using Kakadu.ActionApi.Middleware;
using LazyCache;

namespace Kakadu.ActionApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // this contains default username/password, ideally overriden by environment variables
            ApiConfiguration apiConfiguration = new ApiConfiguration();
            Configuration.GetSection("ConfigurationAPI").Bind(apiConfiguration);

            services.Configure<ApiConfiguration>(Configuration.GetSection("ConfigurationAPI"));

            if(string.IsNullOrWhiteSpace(apiConfiguration.Address))
                throw new Exception("ConfigurationAPI address can't be empty");

            services.AddControllers();

            services.AddHttpClient<IAuthenticationClient, AuthenticationClient>(client => {
                client.BaseAddress = new Uri(apiConfiguration.Address);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // this would issue a new token on each call
            // services.AddTransient<ApiBearerTokenHandler>();
            services.AddScoped<ApiBearerTokenHandler>();
            services.AddHttpClient<IServiceClient, ServiceClient>(client => {
                client.BaseAddress = new Uri(apiConfiguration.Address);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddHttpMessageHandler<ApiBearerTokenHandler>();

            services.AddLazyCache(a => {
                var cache = new CachingService();
                cache.DefaultCachePolicy.DefaultCacheDurationSeconds = 180;

                return cache;
            });
            
            services.AddProxies();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMiddleware<HttpExceptionHandler>();

            app.UseHttpsRedirection();

            app.UseRouting();            

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
        }
    }
}
