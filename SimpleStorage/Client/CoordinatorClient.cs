using System;
using System.Net.Http;

namespace Client
{
    public class CoordinatorClient : ICoordinatorClient
    {
        private readonly string endpoint;

        public CoordinatorClient(string endpoint)
        {
            if (endpoint == null)
                throw new ArgumentException("Empty endpoint!", "endpoint");
            this.endpoint = endpoint;
        }

        public int Get(string id)
        {
            var requestUri = endpoint + "/api/shardMapping/" + id;
            using (var client = new HttpClient())
            using (var response = client.GetAsync(requestUri).Result)
            {
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsAsync<int>().Result;
            }
        }
    }
}