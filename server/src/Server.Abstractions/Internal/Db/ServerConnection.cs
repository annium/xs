
using Annium.Logging;
using LinqToDB;
using LinqToDB.Data;
using Server.Shared.Domain.Interfaces;

namespace Server.Abstractions.Internal.Db;

internal class ServerConnection<TPackage, TPackageDependency> : DataConnection, ILogSubject
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
{
    public ILogger Logger { get; }
    public ITable<TPackage> Packages { get; }

    public ServerConnection(
        DataOptions<ServerConnection<TPackage, TPackageDependency>> config,
        ILogger logger
    ) : base(config.Options)
    {
        Logger = logger;
        Packages = this.GetTable<TPackage>();
    }
}