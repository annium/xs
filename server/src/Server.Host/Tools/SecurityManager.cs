using System;
using System.Security.Cryptography;
using System.Text;

namespace Xs.Registry.Main.Tools;

internal class SecurityManager : ISecurityManager, IDisposable
{
    private readonly HashAlgorithm _hashAlgorithm = new SHA512CryptoServiceProvider();

    public string Hash(string data)
    {
        return Convert.ToBase64String(_hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    #region IDisposable Support

    private bool _disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _hashAlgorithm.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion
}