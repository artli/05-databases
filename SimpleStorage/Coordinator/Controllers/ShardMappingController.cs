using System;
using System.Web.Http;

namespace Coordinator.Controllers
{
    public class ShardMappingController : ApiController
    {
        private IConfiguration config;
        public ShardMappingController(IConfiguration configuration)
        {
            config = configuration;
        }

        public int Get(string id)
        {
            return Math.Abs(id.GetHashCode()) % config.ShardCount;
        }
    }
}