using System;
using Annium.Core.DependencyInjection;
using Server.Shared.Internal;
using Server.Shared.Internal.Tools;
using Server.Shared.Tools;

namespace Server.Shared;

internal class BaseServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddPostgreSql<Connection>();

        // tools
        container.Add<IMetaPackageManager, MetaPackageManager>().Scoped();

        // repositories
        container.AddAll(GetType().Assembly)
            .Where(x => x.IsClass && x.Name.EndsWith("Repository"))
            .AsInterfaces()
            .Scoped();
    }
}