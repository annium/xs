using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Xs.Server.Dotnet.Services;

namespace Annium.Xs.Server.Dotnet.Internal.Services;

internal class SymbolStorage : ISymbolStorage
{
    public Task<bool> ExistsAsync(string name, string version)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> GetFileAsync(string name, string version, string file)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(string name, string version, Stream symbolStream)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string name, string version)
    {
        throw new NotImplementedException();
    }
}
