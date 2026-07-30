using System;

namespace Annium.Xs.Server.Client.Clients;

public abstract class ClientBase
{
    private readonly ClientBase[] _clients;

    protected Uri Uri = new("http://localhost");

    private bool _isUriAssigned;

    public ClientBase(params ClientBase[] clients)
    {
        _clients = clients;
    }

    public void SetUri(Uri uri)
    {
        if (_isUriAssigned)
            throw new InvalidOperationException("Uri already assigned.");

        foreach (var client in _clients)
            client.SetUri(uri);
        Uri = uri;
        _isUriAssigned = true;
    }
}
