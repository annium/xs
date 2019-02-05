using System;

namespace Xs.Registry.Shared.Client
{
    public interface IProjectClientFactory
    {
        IProjectClient Create(Uri uri);
    }
}