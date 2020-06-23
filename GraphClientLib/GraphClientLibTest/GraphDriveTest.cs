using GraphClientLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GraphClientLibTest
{
    public class GraphDriveTest : IClassFixture<GraphHelperMeFixture>
    {
        private IGraphDriveHelper graphClient { get; }
        public GraphDriveTest(GraphHelperMeFixture graphClientFixture)
        {
            graphClient = graphClientFixture.graphDriveHelper;
            
        }
        [Fact]
        public async Task GetDriveIsNotEmpty()
        {
            Assert.NotNull(graphClient);
            var count = 0;
            var token = string.Empty;
            graphClient.ProcessDriveChange = 
                async x => count++;
            graphClient.ProcessToken =
                async t => token = t;

            await graphClient.ProcessAllDriveItems();
            Assert.True(count > 0);
            Assert.False(string.IsNullOrEmpty(token));
        }
    }
}
