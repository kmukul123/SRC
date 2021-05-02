using GraphClientLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;  
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace GraphClientLibTest
{
    public class GraphHelperMeFixture : IDisposable
    {
        public IGraphUserHelper graphUserHelper { get; set; }
        public IGraphDriveHelper graphDriveHelper { get; set; }
        public GraphHelperMeFixture()
        {
            var config = LoadTestConfiguration();
            var diServieProvider = GetDIServiceProvider(config);

            var factory = diServieProvider.GetService<ILoggerFactory>();
            Assert.NotNull(factory);
            var authProvider = diServieProvider.GetService<IAuthenticationProvider>();
            Assert.NotNull(authProvider);
            graphUserHelper = diServieProvider.GetService<IGraphUserHelper>();
            Assert.NotNull(graphUserHelper);
            graphDriveHelper = diServieProvider.GetService<IGraphDriveHelper>();
            Assert.NotNull(graphDriveHelper);
        }

        private ServiceProvider GetDIServiceProvider(IConfiguration config)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(config);


            var serviceProvider = IServiceCollectionExtension.AddInternalServices(services)
                        .AddLogging()
            .BuildServiceProvider();

            return serviceProvider;
        }

        private IConfiguration LoadTestConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: false)
                .AddUserSecrets<GraphHelperMeFixture>()
                .AddEnvironmentVariables()
                .Build();
            // Check for required settings
            if (string.IsNullOrEmpty(config["ClientId"]) ||
                string.IsNullOrEmpty(config["scopes"]))
            {
                throw new ApplicationException("Can find appid or scope");
            }
            return config;
        }

        public void Dispose()
        {
            
        }
    }
}