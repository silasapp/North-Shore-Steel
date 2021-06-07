using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftCore.Helpers
{
    public class CustomRestClient
    {
        private readonly RestClient _client;

        public CustomRestClient(RestClient client)
        {
            _client = client;
        }

        public async Task<(string, Response)> PostAsync<Request, Response>(
            string url,
            Request input,
            string token = null)
        {
            return await CreateRequestAsync<Response>(url, Method.POST, input, token);
        }

        public async Task<(string, Response)> PutAsync<Request, Response>(
            string url,
            Request input,
            string token = null)
        {
            return await CreateRequestAsync<Response>(url, Method.PUT, input, token);
        }

        public async  Task<(string, Response)> GetAsync<Response>(
            string url,
            string token = null)
        {
            return await CreateRequestAsync<Response>(url, Method.GET, token);
        }

        public async Task<(string, Response)> DeleteAsync<Response>(
            string url,
            string token = null)
        {
            return await CreateRequestAsync<Response>(url, Method.DELETE, token);
        }

        #region [ -- Private helper methods -- ]

        private async Task<(string, Response)> CreateRequestAsync<Response>(
            string url,
            Method method,
            string token)
        {
            return await CreateRequestMessageAsync(url, method, token, async (msg) =>
            {
                return await GetResultAsync<Response>(msg);
            });
        }

        private async Task<(string, Response)> CreateRequestAsync<Response>(
            string url,
            Method method,
            object input,
            string token)
        {
            return await CreateRequestMessageAsync(url, method, token, async (req) =>
            {
                if(input != null)
                {
                    var reqKVPairs = JToken.FromObject(input).ToObject<Dictionary<string, object>>();

                    foreach (var kv in reqKVPairs)
                    {
                        req.AddParameter(kv.Key, kv.Value);
                    }

                    //req.AddObject(input);
                }

                return await GetResultAsync<Response>(req);
            });
        }

        private async Task<Response> CreateRequestMessageAsync<Response>(
            string url,
            Method method,
            string token,
            Func<RestRequest, Task<Response>> functor)
        {
            var req = new RestRequest(url, method);

            req.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            if (token != null)
                req.AddHeader("Authorization", $"Bearer {token}");

            return await functor(req);
        }

        private async Task<(string, Response)> GetResultAsync<Response>(RestRequest req)
        {
            var response = await _client.ExecuteAsync(req);

            var content = response.Content;

            if (!response.IsSuccessful)
                return (content, JToken.Parse("{}").ToObject<Response>());

            if (typeof(IConvertible).IsAssignableFrom(typeof(Response)))
                return ("", (Response)Convert.ChangeType(content, typeof(Response)));

            return ("", JToken.Parse(content).ToObject<Response>());
        }

        #endregion
    }
}
