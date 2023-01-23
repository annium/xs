using System.IO;
using Server.Node.Internal;

namespace Server.Node;

internal class Configuration
{
    public const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fff'Z'";

    public readonly string PackagesFolder = Path.Combine("data", Constants.ProjectType.ToString(), "packages");
}