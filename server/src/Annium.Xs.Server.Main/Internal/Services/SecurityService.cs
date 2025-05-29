using System;
using System.Security.Cryptography;
using System.Text;
using Annium.Logging;
using Annium.Xs.Server.Main.Services;

namespace Annium.Xs.Server.Main.Internal.Services;

internal class SecurityService : ISecurityService, IDisposable
{
    private readonly DisposableBox _disposable;
    private readonly HashAlgorithm _hashAlgorithm;

    public SecurityService(ILogger logger)
    {
        _disposable = Disposable.Box(logger);
        _disposable += _hashAlgorithm = SHA512.Create();
    }

    public string Hash(string data)
    {
        return Convert.ToBase64String(_hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}
