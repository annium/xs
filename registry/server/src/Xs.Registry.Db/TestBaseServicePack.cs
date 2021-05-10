using System;
using Annium.Core.DependencyInjection;

namespace Xs.Registry.Db
{
    public class TestBaseServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddEntityFrameworkSqliteInMemory<Context>();
        }
    }
}