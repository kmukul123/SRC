using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace GraphClientLibTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("client-secrets.json")
                .Build();

            //GraphClientLib.Class1 c = null;

        }
    }
}
