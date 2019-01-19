using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Node.Client
{
    public class NodeClientFactory
    {
        private readonly IServiceProvider provider;

        public NodeClientFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public NodeClient Create(Uri uri)
        {
            var client = provider.GetRequiredService<NodeClient>();

            client.SetUri(uri);

            return client;
        }
    }
}