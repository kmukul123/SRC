using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AzureFunction.IOCSample
{
    public interface ICache
    {
    }

    public class Cache : IDisposable, ICache
    {
        private readonly ILogger logger;

        public Cache(ILogger<Cache> logger)
        {
            this.logger = logger;
            logger.LogInformation("****In cache constructor");
        }
        public void Dispose()
        {
            logger.LogInformation("****In cache destruction");
        }
    }
}
