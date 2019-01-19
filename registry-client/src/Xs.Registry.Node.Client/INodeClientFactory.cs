using System;

namespace Xs.Registry.Node.Client
{
    public interface INodeClientFactory
    {
        INodeClient Create(Uri uri);
    }
}