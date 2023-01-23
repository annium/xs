using System;
using Annium.Core.DependencyInjection;
using Server.Abstractions;
using Server.Node.Domain;
using Server.Node.Internal.Services;
using Server.Node.Views.Requests;
using Server.Shared.Auth.TokenAccessors;

namespace Server.Node;

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
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsSelf().Singleton();

        // packages
        container.AddPackageTools<Package, PackageDependency, PackageRequest, PackageRequestParser, PackageStorage>();
    }
}