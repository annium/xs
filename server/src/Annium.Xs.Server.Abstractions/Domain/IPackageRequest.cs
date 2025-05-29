using System.IO;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Abstractions.Domain;

public interface IPackageRequest : IPackageInfo
{
    ProjectType ProjectType { get; }
    Stream Stream { get; }
}
