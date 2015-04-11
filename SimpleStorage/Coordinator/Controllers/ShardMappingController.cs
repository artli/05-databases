using System;
using System.Web.Http;

namespace Coordinator.Controllers
{
    public class ShardMappingController : ApiController
    {
        private IConfiguration configuration;

        public ShardMappingController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public int Get(string id)
        {
            var shardNum = id.GetHashCode() % configuration.ShardCount;
            if (shardNum < 0)
                shardNum += configuration.ShardCount;
            Console.WriteLine("Returning {0} ({1} total shards)", shardNum, configuration.ShardCount);
            return shardNum;
        }
    }
}