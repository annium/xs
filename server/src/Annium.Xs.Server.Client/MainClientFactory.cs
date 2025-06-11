using System;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Xs.Server.Client.Clients;

namespace Annium.Xs.Server.Client;

public class MainClientFactory
{
    private readonly IServiceProvider _provider;

    public MainClientFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public MainClient Create(Uri uri)
    {
        var client = _provider.Resolve<MainClient>();

        client.SetUri(uri);

        return client;
    }
}
