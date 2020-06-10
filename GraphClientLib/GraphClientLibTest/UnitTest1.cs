using GraphClientLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Xunit;

namespace GraphClientLibTest
{
    public class UnitTest1
    {
        [Fact]
        public void TestDependencyInjection()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: false)    
                .AddEnvironmentVariables()
                .Build();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);


            var serviceProvider = IServiceCollectionExtension.AddInternalServices(services)
                        .AddLogging()
            .BuildServiceProvider();


            var factory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(factory);
            var authProvider = serviceProvider.GetService<IAuthenticationProvider>();
            Assert.NotNull(authProvider);
            var graphHelper = serviceProvider.GetService<IGraphHelper>();
            Assert.NotNull(graphHelper);
            //GraphClientLib.Class1 c = null;

        }
    }
}
