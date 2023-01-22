using System;
using Annium.Core.DependencyInjection;
using Server.Abstractions;
using Server.Dotnet.Models;
using Server.Dotnet.Payloads;
using Server.Dotnet.Storage;
using Server.Shared.Auth;
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
        container.AddPackageTools<Package, PackageDependency, PackagePayload, PayloadParser, PackageStorage>();
        container.Add<ISymbolStorage, SymbolStorage>().Singleton();
    }
}