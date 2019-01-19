using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Shared.Client
{
    internal class SharedClientFactory : ISharedClientFactory
    {
        private readonly IServiceProvider provider;

        public SharedClientFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public ISharedClient Create(Uri uri)
        {
            var client = provider.GetRequiredService<ISharedClient>();

            client.SetUri(uri);

            return client;
        }
    }
}