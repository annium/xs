using System;
using System.Security.Cryptography;
using System.Text;

namespace Xs.Registry.Core.Security
{
    internal class SecurityManager : ISecurityManager, IDisposable
    {
        private readonly HashAlgorithm hashAlgorithm = new SHA512CryptoServiceProvider();

        public string Hash(string data)
        {
            return Convert.ToBase64String(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    hashAlgorithm.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}