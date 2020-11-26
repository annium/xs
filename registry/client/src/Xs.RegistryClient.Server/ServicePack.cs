using System;
using Annium.Core.DependencyInjection;

namespace Xs.RegistryClient.Server
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.Add<ServerClientFactory>().AsSelf().Singleton();
            container.Add<ServerClient>().AsSelf().Transient();
        }
    }
}