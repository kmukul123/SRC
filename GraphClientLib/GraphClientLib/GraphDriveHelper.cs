using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Graph.Constants;

namespace GraphClientLib
{
    class GraphDriveHelper : IGraphDriveHelper
    {
        private GraphServiceClient _graphClient;
        private readonly ILogger<GraphUserHelper> _logger;
        private bool _inited;
        private int _pagesize;
        private string _token;

        public GraphDriveHelper(IAuthenticationProvider authProvider, ILogger<GraphUserHelper> logger)
        {
            _graphClient = new GraphServiceClient(authProvider);
            _logger = logger;
            _inited = false;
        }
        public int PageSize
        {
            get => _pagesize;
            set => _pagesize = _inited ? throw new ArgumentException("") : value ;
        }

        public string token
        {
            get => _token;
            set => _token = _inited ? throw new ArgumentException("") : value;
        }
        public Func<string, Task> ProcessToken { get;  set; }
        public Func<DriveItem, Task> ProcessDriveChange { get;  set; }

        /// <summary>
        /// todo : add a way to get next page while current page is being processed
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ProcessAllDriveItems()
        {
            _inited = true;
            IDriveItemDeltaCollectionPage deltaCollection
                   = await _graphClient.Sites.Root.Drive.Root.
                Delta(String.IsNullOrEmpty(token)? null: token).
                Request().
                GetAsync(); ;
            

            if (deltaCollection.CurrentPage.Count <= 0)
            {
                _logger.LogInformation("No changes...");
                return false;
            }
            else
            {
                bool morePagesAvailable = false;
                do
                {
                    // If there is a NextPageRequest, there are more pages
                    morePagesAvailable = deltaCollection.NextPageRequest != null;
                    foreach (var driveItem in deltaCollection.CurrentPage)
                    {
                        if (ProcessDriveChange!=null)
                            await ProcessDriveChange(driveItem);
                    }

                    if (morePagesAvailable)
                    {
                        // Get the next page of results
                        deltaCollection = await deltaCollection.NextPageRequest.GetAsync();
                    }
                }
                while (morePagesAvailable);
            }

            // Once we've iterated through all of the pages, there should
            // be a delta link, which is used to request all changes since our last query
            var deltalink = deltaCollection.AdditionalData["@odata.deltaLink"].ToString();
            _logger.LogInformation($"Processed current delta. getting back link for next delta {deltalink} token={token}");

            var deltauri = new Uri(deltalink);
            var queries = System.Web.HttpUtility.ParseQueryString(deltauri.Query);
            _token = queries.Get("$deltatoken");

            if (ProcessToken != null)
                await (ProcessToken(token));
            return true;
        }

        //original code copies from msdn
        private async Task WatchMailFoldersTest(int pollInterval)
        {
            // Get first page of mail folders
            IMailFolderDeltaCollectionPage deltaCollection;
            deltaCollection = await _graphClient.Me.MailFolders
                .Delta()
                .Request()
                .GetAsync();
            //deltaCollection = await _graphClient.Me.Drive.Root.Delta().Request().GetAsync();
            //await _graphClient.Sites.Root.Drive.Root.Delta().Request().GetAsync()

            while (true)
            {
                if (deltaCollection.CurrentPage.Count <= 0)
                {
                    Console.WriteLine("No changes...");
                }
                else
                {
                    bool morePagesAvailable = false;
                    do
                    {
                        // If there is a NextPageRequest, there are more pages
                        morePagesAvailable = deltaCollection.NextPageRequest != null;
                        foreach (var mailFolder in deltaCollection.CurrentPage)
                        {
                            //await ProcessChanges(mailFolder);
                        }

                        if (morePagesAvailable)
                        {
                            // Get the next page of results
                            deltaCollection = await deltaCollection.NextPageRequest.GetAsync();
                        }
                    }
                    while (morePagesAvailable);
                }

                // Once we've iterated through all of the pages, there should
                // be a delta link, which is used to request all changes since our last query
                _deltaLink = deltaCollection.AdditionalData["@odata.deltaLink"].ToString();
                if (!string.IsNullOrEmpty(_deltaLink))
                {
                    Console.WriteLine($"Processed current delta. Will check back in {pollInterval} seconds.");
                    await Task.Delay(pollInterval * 1000);

                    deltaCollection.InitializeNextPageRequest(_graphClient, _deltaLink.ToString());
                    deltaCollection = await deltaCollection.NextPageRequest.GetAsync();
                }
            }
        }


    }
}
