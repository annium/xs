using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Xs.Registry.Shared.Client
{
    public class SharedClient
    {
        public UserClient User { get; }

        private readonly HttpClient httpClient;

        private Uri uri;

        internal SharedClient(
            UserClient userClient,
            HttpClient httpClient
        )
        {
            this.User = userClient;
            this.httpClient = httpClient;
        }

        internal void SetUri(Uri uri)
        {
            if (this.uri != null)
                throw new InvalidOperationException($"Uri already assigned");

            User.SetUri(uri);
            this.uri = uri;
        }

        public async Task<Dictionary<string, Uri>> GetRegistryInfoAsync(string token)
        {
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            message.RequestUri = new Uri(this.uri, "registry");

            var result = await httpClient.SendAsync(message);
            var response = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"Registry information fetch failed with: {response}");

            return JsonConvert.DeserializeObject<Dictionary<string, Uri>>(response);
        }
    }
}