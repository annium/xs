using System;
using Annium.Core.DependencyInjection;
using Server.Client.Clients;

namespace Server.Client;

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
