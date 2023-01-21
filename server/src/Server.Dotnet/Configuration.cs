using System.IO;

namespace Xs.Registry.Dotnet;

internal class Configuration
{
    public readonly string PackagesFolder = Path.Combine("data", Constants.ProjectType.ToString(), "packages");
}