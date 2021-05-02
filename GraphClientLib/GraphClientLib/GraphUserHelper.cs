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


    public class GraphUserHelper : IGraphUserHelper
    {
        private GraphServiceClient graphClient;
        private readonly ILogger<GraphUserHelper> logger;

        public GraphUserHelper(IAuthenticationProvider authProvider, ILogger<GraphUserHelper> logger)
        {
            graphClient = new GraphServiceClient(authProvider);
            this.logger = logger;
        }


        public async Task<IGraphServiceUsersCollectionPage> GetUsersPagedAsync()
        {
            try
            {
                // GET /me
                var users= await graphClient.Users.Request().GetAsync();
                return users;
            }
            catch (ServiceException ex)
            {
                logger.LogError($"Error getting signed-in user: {ex.Message}");
                throw;
            }
        }
    }
}

