using GraphClientLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
//using System.Text.Json; // doesnt work for dictionary
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GraphClientLibTest
{
    public class GraphDriveTest : IClassFixture<GraphHelperMeFixture>
    {
        private IGraphDriveHelper graphClient { get; }
        private ITestOutputHelper output { get; }

        public GraphDriveTest(GraphHelperMeFixture graphClientFixture, ITestOutputHelper output)
        {
            graphClient = graphClientFixture.graphDriveHelper;
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
                    count++; 
                    var jsonString = JsonConvert.SerializeObject(x, Formatting.Indented); ;
                    output.WriteLine(jsonString);
                };
            graphClient.ProcessToken =
                async t => token = t;

            await graphClient.ProcessAllDriveItems();
            Assert.True(count > 0);
            Assert.False(string.IsNullOrEmpty(token));
        }
    }
}
