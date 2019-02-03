using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Tools;
using Xs.Registry.Node.Models;
using Xs.Registry.Node.Storage;

namespace Xs.Registry.Node
{
    public class BaseServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // auth
            services.AddSingleton<ITokenAccessor, BearerTokenAccessor>();

            // storage
            services.AddSingleton<IPackageStorage, PackageStorage>();

            // tools
            services.AddSingleton<ISearchManager, SearchManager<Package>>();
        }
    }
}