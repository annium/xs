using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Xs.Registry.Shared.Client
{
    internal class SharedClient : ISharedClient
    {
        private Uri uri;

        private readonly HttpClient httpClient;

        public SharedClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public void SetUri(Uri uri)
        {
            if (this.uri != null)
                throw new InvalidOperationException($"Uri already assigned");

            this.uri = uri;
        }

        public async Task<string> CreateUserAsync(string name, string password)
        {
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Put;
            message.RequestUri = new Uri(this.uri, "user");
            message.Content = new StringContent(JsonConvert.SerializeObject(new { name, password }), Encoding.UTF8, "application/json");

            var result = await httpClient.SendAsync(message);
            var response = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"User creation failed with: {response}");

            return response;
        }

        public async Task<string> UpdateUserAsync(string token, string newPassword)
        {
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Post;
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            message.RequestUri = new Uri(this.uri, "user");
            message.Content = new StringContent(JsonConvert.SerializeObject(new { newPassword }), Encoding.UTF8, "application/json");

            var result = await httpClient.SendAsync(message);
            var response = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"User update failed with: {response}");

            return response;
        }

        public async Task<string> LoginUserAsync(string name, string password)
        {
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(this.uri, $"user?name={name}&password={password}");

            var result = await httpClient.SendAsync(message);
            var response = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"User login failed with: {response}");

            return response;
        }

        public async Task DeleteUserAsync(string token)
        {
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Delete;
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            message.RequestUri = new Uri(this.uri, "user");

            var result = await httpClient.SendAsync(message);
            var response = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode)
                throw new InvalidOperationException($"User delete failed with: {response}");
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