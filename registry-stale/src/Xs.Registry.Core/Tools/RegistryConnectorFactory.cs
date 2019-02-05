using System;


namespace Xs.Registry.Core.Tools
{
    internal class RegistryConnectorFactory : IRegistryConnectorFactory
    {
        public IRegistryConnector Create(Uri sharedUri, ProjectType type, Uri uri)
        {
            return new RegistryConnector(sharedUri, type, uri);
        }
    }
}