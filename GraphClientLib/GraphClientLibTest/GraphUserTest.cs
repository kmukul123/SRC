using GraphClientLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GraphClientLibTest
{
    public class GraphUserTest : IClassFixture<GraphHelperMeFixture>
    {
        private IGraphUserHelper graphClient { get; }

        public GraphUserTest(GraphHelperMeFixture graphClientFixture)
        {
            this.graphClient = graphClientFixture.graphUserHelper;
        }


        [Fact]
        public async Task GetUserIsNotNull()
        {
            Assert.NotNull(graphClient);
            var user = await this.graphClient.GetUsersPagedAsync();
            Assert.NotNull(user);
            Assert.True(user.CurrentPage.Count > 0);
        }
    }
}
