using System;
using System.IO;
using System.Threading.Tasks;

namespace Xs.Registry.Dotnet.Storage;

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