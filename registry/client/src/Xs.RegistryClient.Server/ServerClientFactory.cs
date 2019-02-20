using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.RegistryClient.Server
{
    public class ServerClientFactory
    {
        private readonly IServiceProvider provider;

        public ServerClientFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public ServerClient Create(Uri uri)
        {
            var client = provider.GetRequiredService<ServerClient>();

            client.SetUri(uri);

            return client;
        }
    }
}