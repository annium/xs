using System;

namespace Xs.Registry.Core.Security
{
    internal class TokenManager : ITokenManager
    {
        public string CreateToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}