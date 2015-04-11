using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Domain;

namespace Client
{
    public class SimpleStorageClient : ISimpleStorageClient
    {
        private readonly List<string> endpoints;
        private readonly CoordinatorClient coordinatorClient;

        public SimpleStorageClient(params string[] endpoints)
        {
            coordinatorClient = new CoordinatorClient("http://127.0.0.1:17000");
            if (endpoints == null || !endpoints.Any())
                throw new ArgumentException("Empty endpoints!", "endpoints");
            this.endpoints = endpoints.OrderBy(x => x).ToList();
        }

        public void Put(string id, Value value)
        {
            var shardNum = coordinatorClient.Get(id);
            var putUri = endpoints[shardNum] + "api/values/" + id;
            using (var client = new HttpClient())
            using (var response = client.PutAsJsonAsync(putUri, value).Result)
                response.EnsureSuccessStatusCode();
        }

        public Value Get(string id)
        {
            var shardNum = coordinatorClient.Get(id);
            var requestUri = endpoints[shardNum] + "api/values/" + id;
            using (var client = new HttpClient())
            using (var response = client.GetAsync(requestUri).Result)
            {
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsAsync<Value>().Result;
            }
        }
    }
}