using Annium.linq2db.Extensions.Models;
using Annium.Logging.Abstractions;
using LinqToDB;
using LinqToDB.Data;
using Server.Domain.Models;

namespace Server.Db.Internal;

internal class Connection : DataConnection, ILogSubject<Connection>
{
    public ILogger<Connection> Logger { get; }
    public ITable<MetaPackage> MetaPackages { get; set; }
    public ITable<User> Users { get; set; }
    public ITable<UserSession> UserSessions { get; set; }

    public Connection(
        Config<Connection> config,
        ILogger<Connection> logger
    ) : base(config.Options)
    {
        Logger = logger;
        MetaPackages = this.GetTable<MetaPackage>();
        Users = this.GetTable<User>();
        UserSessions = this.GetTable<UserSession>();
    }
}