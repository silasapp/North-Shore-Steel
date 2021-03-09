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
        readonly RestClient _client;

        public CustomRestClient(RestClient client)
        {
            _client = client;
        }

        public (string, Response) PostAsync<Request, Response>(
            string url,
            Request input,
            string token = null)
        {
            return CreateRequest<Response>(url, Method.POST, input, token);
        }

        public (string, Response) PutAsync<Request, Response>(
            string url,
            Request input,
            string token = null)
        {
            return CreateRequest<Response>(url, Method.POST, input, token);
        }

        public (string, Response) GetAsync<Response>(
            string url,
            string token = null)
        {
            return CreateRequest<Response>(url, Method.GET, token);
        }

        public (string, Response) DeleteAsync<Response>(
            string url,
            string token = null)
        {
            return CreateRequest<Response>(url, Method.DELETE, token);
        }

        #region [ -- Private helper methods -- ]

        (string, Response) CreateRequest<Response>(
            string url,
            Method method,
            string token)
        {
            return CreateRequestMessage(url, method, token, async (msg) =>
            {
                return GetResult<Response>(msg);
            });
        }

        (string, Response) CreateRequest<Response>(
            string url,
            Method method,
            object input,
            string token)
        {
            return CreateRequestMessage(url, method, token, async (req) =>
            {
                var reqKVPairs = JToken.FromObject(input).ToObject<Dictionary<string, object>>();

                foreach (var kv in reqKVPairs)
                {
                    req.AddParameter(kv.Key, kv.Value);
                }
                //req.AddObject(input);

                return GetResult<Response>(req);
            });
        }

        Response CreateRequestMessage<Response>(
            string url,
            Method method,
            string token,
            Func<RestRequest, Task<Response>> functor)
        {
            var req = new RestRequest(url, method);

            req.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            if (token != null)
                req.AddHeader("Authorization", $"Bearer {token}");

            return functor(req).Result;
        }

        (string, Response) GetResult<Response>(RestRequest req)
        {
            var response = _client.Execute(req);

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
