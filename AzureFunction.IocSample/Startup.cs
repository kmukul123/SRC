using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(AzureFunction.IOCSample.StartUp))]

namespace AzureFunction.IOCSample
{
    public class StartUp : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.
            //builder.Services.AddHttpClient();
            
            builder.Services.AddSingleton<ICache, Cache>();
        }
    }


}
