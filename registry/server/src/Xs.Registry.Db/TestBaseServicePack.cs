using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db
{
    public class TestBaseServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddEntityFrameworkSqliteInMemory<Context>();
        }
    }
}