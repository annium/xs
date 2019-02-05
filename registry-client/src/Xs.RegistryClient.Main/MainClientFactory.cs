using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.RegistryClient.Main
{
    public class MainClientFactory
    {
        private readonly IServiceProvider provider;

        public MainClientFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public MainClient Create(Uri uri)
        {
            var client = provider.GetRequiredService<MainClient>();

            client.SetUri(uri);

            return client;
        }
    }
}