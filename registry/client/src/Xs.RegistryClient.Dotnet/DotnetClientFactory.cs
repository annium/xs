using System;
using Microsoft.Extensions.DependencyInjection;
using Xs.RegistryClient.Shared;

namespace Xs.RegistryClient.Dotnet
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