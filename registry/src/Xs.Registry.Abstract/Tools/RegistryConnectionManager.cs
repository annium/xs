using Microsoft.AspNetCore.Hosting;
using Xs.Core.Models;

namespace Xs.Registry.Abstract.Tools
{
    internal class RegistryConnectionManager : IRegistryConnectionManager
    {
        public void Setup(ProjectType type, IConfiguration configuration, IApplicationLifetime lifetime)
        {
            var connector = new RegistryConnector(type, configuration.Location, configuration.MainLocation);
            lifetime.ApplicationStarted.Register(connector.Connect);
            lifetime.ApplicationStopping.Register(connector.Disconnect);
        }
    }
}