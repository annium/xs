using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Node.Domain;

namespace Annium.Xs.Server.Node.Services;

public interface IPackageStorage : IPackageStorage<Package, PackageDependency>;
