using System.Net;
using System.Web.Http;
using Domain;
using SimpleStorage.Infrastructure;
using Client;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SimpleStorage.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly IConfiguration configuration;
        private readonly IStateRepository stateRepository;
        private readonly IStorage storage;
        private readonly List<InternalClient> clients;

        public ValuesController(IStorage storage, IStateRepository stateRepository, IConfiguration configuration)
        {
            this.storage = storage;
            this.stateRepository = stateRepository;
            this.configuration = configuration;
            clients = configuration.OtherShardsPorts
                .Select(port => new InternalClient("http://127.0.0.1:" + port.ToString() + "/"))
                .ToList();
        }

        private void CheckState()
        {
            if (stateRepository.GetState() != State.Started)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
        }

        private IEnumerable<InternalClient> MixClients() {
            var rnd = new Random();
            return clients.OrderBy(client => rnd.NextDouble());
        }

        private Value GetLocal(string id) {
            CheckState();
            return storage.Get(id);
        }

        // GET api/values/5 
        public Value Get(string id)
        {
            var quorum = clients.Count / 2; 
            var results = new List<Value>();
            results.Add(GetLocal(id));
            foreach (var client in MixClients()) {
                if (results.Count >= quorum)
                    break;
                try {
                    results.Add(client.Get(id));
                } catch (HttpResponseException e) {
                    if (e.Response.StatusCode == HttpStatusCode.NotFound)
                        results.Add(null);
                }
            }
            if (results.Count < quorum)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            var bestAnswer = results.OrderBy(result => result == null ? long.MinValue : result.Revision).ElementAt(0);
            return bestAnswer;
        }

        private void PutLocal(string id, Value value) {
            CheckState();
            storage.Set(id, value);
        }

        // PUT api/values/5
        public void Put(string id, [FromBody] Value value)
        {
            var successfulAnswersRemaining = clients.Count / 2;
            PutLocal(id, value);
            foreach (var client in MixClients()) {
                if (successfulAnswersRemaining <= 0)
                    break;
                try {
                    client.Put(id, value);
                } catch (HttpResponseException) {
                    continue;
                }
                successfulAnswersRemaining--;
            }
            if (successfulAnswersRemaining > 0)
                throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}