using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.RegistryClient.Main
{
    public class MainClientFactory
    {
        private readonly IServiceProvider _provider;

        public MainClientFactory(
            IServiceProvider provider
        )
        {
            _provider = provider;
        }

        public MainClient Create(Uri uri)
        {
            var client = _provider.GetRequiredService<MainClient>();

            client.SetUri(uri);

            return client;
        }
    }
}