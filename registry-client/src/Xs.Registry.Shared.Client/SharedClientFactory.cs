using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Shared.Client
{
    public class SharedClientFactory
    {
        private readonly IServiceProvider provider;

        public SharedClientFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public SharedClient Create(Uri uri)
        {
            var client = provider.GetRequiredService<SharedClient>();

            client.SetUri(uri);

            return client;
        }
    }
}