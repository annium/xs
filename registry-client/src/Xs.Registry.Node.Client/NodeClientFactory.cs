using System;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Shared.Client;

namespace Xs.Registry.Node.Client
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