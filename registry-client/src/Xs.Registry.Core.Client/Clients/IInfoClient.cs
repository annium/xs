using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xs.Registry.Core.Client
{
    public interface IInfoClient
    {
        Task<IReadOnlyDictionary<string, string>> SearchAsync(string query, string token);

        Task<string> GetInfoAsync(string name, string token);
    }
}