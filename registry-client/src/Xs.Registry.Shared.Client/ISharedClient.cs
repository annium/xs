using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xs.Registry.Shared.Client
{
    public interface ISharedClient
    {
        void SetUri(Uri uri);

        Task<string> CreateUserAsync(string name, string password);

        Task<string> LoginUserAsync(string name, string password);

        Task<string> UpdateUserAsync(string token, string newPassword);

        Task DeleteUserAsync(string token);

        Task<Dictionary<string, Uri>> GetRegistryInfoAsync(string token);
    }
}