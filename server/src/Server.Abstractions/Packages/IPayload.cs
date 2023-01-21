using System.IO;
using Server.Db.Shared.Models;

namespace Server.Abstractions.Packages;

public interface IPayload : IPackageInfo
{
    Stream Stream { get; }
}