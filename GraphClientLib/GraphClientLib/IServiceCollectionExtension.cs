using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("GraphClientLibTest")]
namespace GraphClientLib
{
    public static class IServiceCollectionExtension
    {
        internal static IServiceCollection AddInternalServices(this IServiceCollection services)
        {
            //services.AddTransient<IWriteExtendedDataService, WriteExtendedDataService>();
            services.AddLogging(config =>
            {
                config.AddDebug();      // Log to debug (debug window in Visual Studio or any debugger attached)
                config.AddConsole();    // Log to console (colored !)
            });
            services.AddTransient<IAuthenticationProvider, GraphAuthenticationProvider>();
            services.AddTransient<IGraphServiceClient, GraphServiceClient>();
            services.AddTransient<IGraphUserHelper, GraphUserHelper>();
            services.AddHttpClient("RestClient")
                    .AddPolicyHandler((serviceProvider, request) => HttpPolicyExtensions.HandleTransientHttpError()
                            .WaitAndRetryAsync(new[]
                            {
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(5),
                                TimeSpan.FromSeconds(10),
                                TimeSpan.FromSeconds(20),
                                TimeSpan.FromSeconds(30)
                            }
                        ));
            return services;
        }
    }
}

