using System;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Shared.Client;

namespace Xs.Registry.Dotnet.Client
{
    public class DotnetClientFactory : IProjectClientFactory
    {
        private readonly IServiceProvider provider;

        public DotnetClientFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public IProjectClient Create(Uri uri)
        {
            var client = provider.GetRequiredService<DotnetClient>();

            client.SetUri(uri);

            return client;
        }
    }
}