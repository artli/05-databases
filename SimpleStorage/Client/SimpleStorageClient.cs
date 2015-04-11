using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Domain;
using System.Web.Http;

namespace Client
{
    public class SimpleStorageClient : ISimpleStorageClient
    {
        private readonly List<string> endpoints;
        private int nextEndpointNum = 0;
        private string nextEndpoint {
            get {
                var oldNum = nextEndpointNum;
                nextEndpointNum = (nextEndpointNum + 1) % endpoints.Count;
                return endpoints[nextEndpointNum];
            }
        }

        public SimpleStorageClient(params string[] endpoints)
        {
            if (endpoints == null || !endpoints.Any())
                throw new ArgumentException("Empty endpoints!", "endpoints");
            this.endpoints = endpoints.ToList();
        }

        public void Put(string id, Value value)
        {
            while (true) {
                var putUri = nextEndpoint + "api/values/" + id;
                using (var client = new HttpClient())
                using (var response = client.PutAsJsonAsync(putUri, value).Result)
                    try {
                        response.EnsureSuccessStatusCode();
                    } catch (HttpRequestException) {
                        continue;
                    }
                break;
            }
        }

        public Value Get(string id)
        {
            while (true) {
                var requestUri = nextEndpoint + "api/values/" + id;
                using (var client = new HttpClient())
                using (var response = client.GetAsync(requestUri).Result) {
                    try {
                        response.EnsureSuccessStatusCode();
                    } catch (HttpRequestException) {
                        continue;
                    }
                    return response.Content.ReadAsAsync<Value>().Result;
                }
            }
        }
    }
}