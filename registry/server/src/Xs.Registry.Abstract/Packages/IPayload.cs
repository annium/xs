using System.IO;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Abstract.Packages;

public interface IPayload : IPackageInfo
{
    Stream Stream { get; }
}