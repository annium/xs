using System;
using Microsoft.Extensions.DependencyInjection;
using Xs.RegistryClient.Shared;

namespace Xs.RegistryClient.Node
{
    public class NodeClientFactory : IProjectClientFactory
    {
        private readonly IServiceProvider provider;

        public NodeClientFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public IProjectClient Create(Uri uri)
        {
            var client = provider.GetRequiredService<NodeClient>();

            client.SetUri(uri);

            return client;
        }
    }
}