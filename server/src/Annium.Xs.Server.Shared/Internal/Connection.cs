using Annium.Logging;
using Annium.Xs.Server.Shared.Domain.Models;
using LinqToDB;
using LinqToDB.Data;

namespace Annium.Xs.Server.Shared.Internal;

internal class Connection : DataConnection, ILogSubject
{
    public ILogger Logger { get; }
    public ITable<MetaPackage> MetaPackages { get; set; }
    public ITable<MetaPackagePermission> MetaPackagePermissions { get; set; }
    public ITable<User> Users { get; set; }

    public Connection(DataOptions<Connection> config, ILogger logger)
        : base(config.Options)
    {
        Logger = logger;
        MetaPackages = this.GetTable<MetaPackage>();
        MetaPackagePermissions = this.GetTable<MetaPackagePermission>();
        Users = this.GetTable<User>();
    }
}
