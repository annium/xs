using System.IO;
using Server.Shared.Domain.Interfaces;
using Server.Shared.Domain.Models;

namespace Server.Abstractions.Domain;

public interface IPackageRequest : IPackageInfo
{
    ProjectType ProjectType { get; }
    Stream Stream { get; }
}