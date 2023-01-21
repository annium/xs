using System.IO;
using Server.Domain.Models;

namespace Server.Abstractions.Packages;

public interface IPayload : IPackageInfo
{
    Stream Stream { get; }
}