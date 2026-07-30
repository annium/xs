using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Main.Internal.Services;
using Annium.Xs.Server.Main.Services;

namespace Annium.Xs.Server.Main;

public class ServicePack : ServicePackBase
{
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        // services
        container.Add<IMetaPackageService, MetaPackageService>().Singleton();
        container.Add<IUserService, UserService>().Singleton();

        // tools
        container.Add<ISecurityService, SecurityService>().Singleton();

        return Task.CompletedTask;
    }
}
