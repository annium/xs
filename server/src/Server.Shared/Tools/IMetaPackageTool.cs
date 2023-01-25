using Server.Shared.Domain.Interfaces;
using Server.Shared.Domain.Models;

namespace Server.Shared.Tools;

public interface IMetaPackageTool
{
    MetaPackage Generate(User user, ProjectType type, IPackageInfo package);
    MetaPackageAccess GetAccess(MetaPackage metaPackage);
}