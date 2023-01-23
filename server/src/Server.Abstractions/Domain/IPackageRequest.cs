using System.IO;
using Server.Domain.Interfaces;
using Server.Domain.Models;

namespace Server.Abstractions.Domain;

public interface IPackageRequest : IPackageInfo
{
    ProjectType ProjectType { get; }
    Stream Stream { get; }
}