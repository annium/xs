using System.IO;
using Server.Domain.Interfaces;

namespace Server.Abstractions.Domain;

public interface IPayload : IPackageInfo
{
    Stream Stream { get; }
}