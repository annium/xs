using System;


namespace Xs.Registry.Core.Tools
{
    public interface IRegistryConnectorFactory
    {
        IRegistryConnector Create(Uri sharedUri, ProjectType type, Uri uri);
    }
}