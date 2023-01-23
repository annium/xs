using System;
using Annium.Core.DependencyInjection;
using Server.Abstractions;
using Server.Dotnet.Domain;
using Server.Dotnet.Internal.Services;
using Server.Dotnet.Services;
using Server.Dotnet.Views.Requests;
using Server.Shared.Auth.TokenAccessors;

namespace Server.Dotnet;

public class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.Add(new Configuration()).AsSelf().Singleton();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // TODO: setup with index

        // auth
        container.Add<ITokenAccessor>(new HeaderTokenAccessor("X-NuGet-ApiKey")).AsInterfaces().Singleton();
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsInterfaces().Singleton();

        // packages
        container.AddPackageTools<Package, PackageDependency, PackageRequest, PackageRequestParser, PackageStorage>();
        container.Add<ISymbolStorage, SymbolStorage>().Singleton();
    }
}