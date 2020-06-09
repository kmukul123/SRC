using System;
using System.Collections.Generic;
using System.Text;

namespace GraphClientLib
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace GraphTutorial
    {
        public class GraphHelperMe : IGraphHelperMe
        {
            private GraphServiceClient graphClient;
            private readonly ILogger<GraphHelperMe> logger;

            public GraphHelperMe(IAuthenticationProvider authProvider, ILogger<GraphHelperMe> logger)
            {
                graphClient = new GraphServiceClient(authProvider);
                this.logger = logger;
            }

            
            public async Task<User> GetMeAsync()
            {
                try
                {
                    // GET /me
                    return await graphClient.Me.Request().GetAsync();
                }
                catch (ServiceException ex)
                {
                    logger.LogError($"Error getting signed-in user: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
