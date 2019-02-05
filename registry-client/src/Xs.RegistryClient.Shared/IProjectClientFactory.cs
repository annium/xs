using System;

namespace Xs.RegistryClient.Shared
{
    public interface IProjectClientFactory
    {
        IProjectClient Create(Uri uri);
    }
}