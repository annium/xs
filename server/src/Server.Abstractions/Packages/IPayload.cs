using System.IO;
using Server.Domain.Interfaces;

namespace Server.Abstractions.Packages;

public interface IPayload : IPackageInfo
{
    Stream Stream { get; }
}