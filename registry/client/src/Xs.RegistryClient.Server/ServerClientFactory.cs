using System;
using Annium.Core.DependencyInjection;

namespace Xs.RegistryClient.Server
{
    public class ServerClientFactory
    {
        private readonly IServiceProvider _provider;

        public ServerClientFactory(
            IServiceProvider provider
        )
        {
            _provider = provider;
        }

        public ServerClient Create(Uri uri)
        {
            var client = _provider.Resolve<ServerClient>();

            client.SetUri(uri);

            return client;
        }
    }
}