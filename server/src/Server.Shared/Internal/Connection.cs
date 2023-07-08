using Annium.Logging.Abstractions;
using LinqToDB;
using LinqToDB.Data;
using Server.Shared.Domain.Models;

namespace Server.Shared.Internal;

internal class Connection : DataConnection, ILogSubject<Connection>
{
    public ILogger<Connection> Logger { get; }
    public ITable<MetaPackage> MetaPackages { get; set; }
    public ITable<MetaPackagePermission> MetaPackagePermissions { get; set; }
    public ITable<User> Users { get; set; }

    public Connection(
        DataOptions<Connection> config,
        ILogger<Connection> logger
    ) : base(config.Options)
    {
        Logger = logger;
        MetaPackages = this.GetTable<MetaPackage>();
        MetaPackagePermissions = this.GetTable<MetaPackagePermission>();
        Users = this.GetTable<User>();
    }
}