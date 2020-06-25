using GraphClientLib;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
//using System.Text.Json; // doesnt work for dictionary
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GraphClientLibTest
{
    public class GraphDriveTest : IClassFixture<GraphHelperMeFixture>
    {
        private GraphDriveHelper graphClient { get; }
        private ITestOutputHelper output { get; }

        public GraphDriveTest(GraphHelperMeFixture graphClientFixture, ITestOutputHelper output)
        {
            graphClient = graphClientFixture.graphDriveHelper as GraphDriveHelper;
            this.output = output;
        }
        [Fact]
        public async Task GetDriveIsNotEmpty()
        {
            Assert.NotNull(graphClient);
            var count = 0;
            var token = string.Empty;
            graphClient.ProcessDriveChange = 
                async x => { 
                    //var jsonString = JsonConvert.SerializeObject(x, Formatting.Indented); ;
                    //output.WriteLine(jsonString);
                    output.WriteLine($"id {x.Id} \tname {x.Name} \tUrl {x.WebUrl}");
                };
            graphClient.ProcessDeltaLink =
                async t => token = t;

                    count++;
            count = await graphClient.ProcessAllDriveItems();
            Assert.True(count > 0);
            Assert.False(string.IsNullOrEmpty(token));
        }
        [Fact]
        public async Task NewChangesAreRetrivedWithToken()
        {
            Assert.NotNull(graphClient);
            var token = string.Empty;
            DriveItem firstFolder=null;
            graphClient.ProcessDriveChange =
                async x => {
                    var jsonString = JsonConvert.SerializeObject(x, Formatting.Indented);
                    if (x.Folder != null && firstFolder == null)
                        firstFolder = x;
                    //output.WriteLine(jsonString);
                    var item = $"id {x.Id} \tname {x.Name} \tUrl {x.WebUrl}";
                    log(item);
                };
            graphClient.ProcessDeltaLink =
                async t => token = t;

            var count1 = await graphClient.ProcessAllDriveItems();
            
            Assert.True(count1 > 0);
            Assert.False(string.IsNullOrEmpty(token));

            var stream = new MemoryStream(Encoding.ASCII.GetBytes("Hello World"));
            var uploadedItem = await graphClient.UploadSmallFile(firstFolder, stream, "Hello.txt");
            log($"      uploaded file {uploadedItem.Id} {uploadedItem.Name}");
            log($"      Looking for changes");
            var foundHelloChange = false;
            graphClient.ProcessDriveChange =
                async x => {
                    //output.WriteLine(jsonString);
                    var item = $"id {x.Id} \tname {x.Name} \tUrl {x.WebUrl}";
                    if (x.Id == uploadedItem.Id)
                        foundHelloChange = true;
                    log(item);
                };

            try
            {
                var count2 = await graphClient.ProcessAllDriveItems();
                log($"      found {count2} changes");

                Assert.True(count2 > 0 && count2< count1+1);
                Assert.True(foundHelloChange);
                Assert.NotNull(uploadedItem);
                Assert.False(string.IsNullOrEmpty(token));
            } finally
            {
                await graphClient._graphClient
               .Drives[uploadedItem.ParentReference.DriveId]
               .Items[uploadedItem.Id]
               .Request()
               .DeleteAsync();
                log($"      deleted file {uploadedItem.Id} {uploadedItem.Name}");
            }
            var count3 = await graphClient.ProcessAllDriveItems();
            log($"      found {count3} changes");

            var count4 = await graphClient.ProcessAllDriveItems();
            log($"      found {count4} changes");
            Assert.Equal(0,count4);

        }

        private void log(string item)
        {
            output.WriteLine(item);
            Debug.WriteLine(item);
        }
    }
}
