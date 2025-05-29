using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Shared.Tools;

public interface IMetaPackageTool
{
    MetaPackage Generate(User user, ProjectType type, IPackageInfo package);
    MetaPackageAccess GetAccess(MetaPackage metaPackage);
}
