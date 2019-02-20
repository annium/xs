using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.RegistryClient.Abstract
{
    public class AbstractClientFactory
    {
        private readonly IServiceProvider provider;

        public AbstractClientFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public AbstractClient Create(Uri uri)
        {
            var client = provider.GetRequiredService<AbstractClient>();

            client.SetUri(uri);

            return client;
        }
    }
}