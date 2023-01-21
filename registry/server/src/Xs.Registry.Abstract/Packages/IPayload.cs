using System.IO;
using Xs.Registry.Db.Shared.Models;

namespace Xs.Registry.Abstract.Packages;

public interface IPayload : IPackageInfo
{
    Stream Stream { get; }
}