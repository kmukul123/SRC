using GraphClientLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GraphClientLibTest
{
    public class GraphHelperMeTest : IClassFixture<GraphHelperMeFixture>
    {
        private IGraphUserHelper graphClient { get; }

        public GraphHelperMeTest(GraphHelperMeFixture graphClientFixture)
        {
            this.graphClient = graphClientFixture.meGraphHelper;
        }


        [Fact]
        public async Task GetUserIsNotNull()
        {
            var user = await this.graphClient.GetUsersPagedAsync();
            Assert.NotNull(user);
            Assert.True(user.CurrentPage.Count > 0);
        }
    }
}
