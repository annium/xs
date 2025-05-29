using System.IO;
using Annium.Xs.Server.Dotnet.Internal;

namespace Annium.Xs.Server.Dotnet;

internal class Configuration
{
    public readonly string PackagesFolder = Path.Combine("data", Constants.ProjectType.ToString(), "packages");
}
