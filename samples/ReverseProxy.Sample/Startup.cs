// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ReverseProxy.Core.Abstractions;

namespace Microsoft.ReverseProxy.Sample
{
    /// <summary>
    /// ASP .NET Core pipeline initialization.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMemoryCache();
            services.AddReverseProxy()
                .LoadFromConfig(_configuration.GetSection("ReverseProxy"), reloadOnChange: true)
                .ConfigureBackendDefaults((id, backend) =>
                {
                    backend.HealthCheckOptions ??= new HealthCheckOptions();
                    // How to use custom metadata to configure backends
                    if (backend.Metadata?.TryGetValue("CustomHealth", out var customHealth) ?? false
                        && string.Equals(customHealth, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        backend.HealthCheckOptions.Enabled = true;
                    }

                    // Or wrap the meatadata in config sugar
                    var config = new ConfigurationBuilder().AddInMemoryCollection(backend.Metadata).Build();
                    backend.HealthCheckOptions.Enabled = config.GetValue<bool>("CustomHealth");
                })
                .ConfigureBackend("backend1", backend =>
                {
                    backend.HealthCheckOptions.Enabled = false;
                })
                .ConfigureRouteDefaults(route =>
                {
                    // Do not let config based routes take priority over code based routes.
                    // Lower numbers are higher priority.
                    if (route.Priority.HasValue && route.Priority.Value < 0)
                    {
                        route.Priority = 0;
                    }
                })
                // If I need services as part of the config:
                .ConfigureRoute<IMemoryCache>("route1", (route, cache) =>
                {
                    var value = cache.Get<int>("key");
                    route.Priority = value;
                })
                ;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapReverseProxy(proxyPipeline =>
                {
                    proxyPipeline.UseProxyLoadBalancing();
                    // Customize the request before forwarding
                    proxyPipeline.Use((context, next) =>
                    {
                        var connection = context.Connection;
                        context.Request.Headers.AppendCommaSeparatedValues("X-Forwarded-For",
                            new IPEndPoint(connection.RemoteIpAddress, connection.RemotePort).ToString());
                        return next();
                    });
                });
            });
        }
    }
}
