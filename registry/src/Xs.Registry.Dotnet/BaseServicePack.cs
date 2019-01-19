using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Core.Auth;
using Xs.Registry.Dotnet.Auth;
using Xs.Registry.Dotnet.Storage;

namespace Xs.Registry.Dotnet
{
    public class BaseServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // auth
            services.AddSingleton<ITokenAccessor, TokenAccessor>();

            // storage
            services.AddSingleton<IPackageStorage, PackageStorage>();
            services.AddSingleton<ISymbolStorage, SymbolStorage>();
        }
    }
}