using System;

namespace Xs.Registry.Shared.Client
{
    public interface ISharedClientFactory
    {
        ISharedClient Create(Uri uri);
    }
}