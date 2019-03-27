using System.IO;
using System.Linq;
using System.Reflection;

namespace Xs.Cli.Core.Helpers
{
    public static class ResourceLoader
    {
        public static Resource[] Load(string prefix, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();

            prefix = $"{assembly.GetName().Name}.{prefix}";

            return assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(prefix))
                .Select(r =>
                {
                    var name = r.Substring(prefix.Length + 1);
                    var rs = assembly.GetManifestResourceStream(r);
                    rs.Seek(0, SeekOrigin.Begin);

                    return new Resource(name, rs);
                })
                .ToArray();
        }
    }
}