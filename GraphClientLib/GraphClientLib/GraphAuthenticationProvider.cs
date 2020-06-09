using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GraphClientLib
{
    /// <summary>
    /// added from http://www.keithmsmith.com/get-started-microsoft-graph-api-calls-net-core-3/
    /// </summary>
    public class GraphAuthenticationProvider : IAuthenticationProvider
    {
       
        private AuthenticationConfig authConfig;

        public GraphAuthenticationProvider(IConfiguration configuration)
        {
            authConfig = new AuthenticationConfig(configuration);
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            AuthenticationContext authContext = new AuthenticationContext(authConfig.Authority);

            ClientCredential creds = new ClientCredential(authConfig.ClientId, authConfig.ClientSecret);

            AuthenticationResult authResult = await authContext.AcquireTokenAsync(AuthenticationConfig.ApiUrl, creds);

            request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);
        }
    }
}
