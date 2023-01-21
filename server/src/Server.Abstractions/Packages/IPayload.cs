using System.IO;
using Xs.Registry.Db.Shared.Models;

namespace Server.Abstractions.Packages;

public interface IPayload : IPackageInfo
{
    Stream Stream { get; }
}