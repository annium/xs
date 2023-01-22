using System;
using Annium.Core.DependencyInjection;
using Server.Shared.Internal.Tools;
using Server.Shared.Tools;

namespace Server.Shared;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // tools
        container.Add<IMetaPackageManager, MetaPackageManager>().Scoped();
    }
}