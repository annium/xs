using Server.Domain.Interfaces;
using Server.Domain.Models;

namespace Server.Db.Shared.Tools;

public interface IMetaPackageManager
{
    MetaPackage Generate(User user, ProjectType type, IPackageInfo package);

    MetaPackageAccess GetAccess(MetaPackage metaPackage);
}