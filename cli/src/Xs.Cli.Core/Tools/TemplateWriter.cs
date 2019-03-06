using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;
using Xs.Cli.Core.Helpers;

namespace Xs.Cli.Core.Tools
{
    internal class TemplateWriter : ITemplateWriter
    {
        private string root = Directory.GetCurrentDirectory();

        private Resource[] resources;

        public void LoadResources(string prefix)
        {
            resources = ResourceLoader.Load(prefix, Assembly.GetCallingAssembly());
        }

        public void SetRoot(string root)
        {
            Directory.CreateDirectory(root);
            this.root = root;
        }

        public void Write(string resourceName, string fileName, object data)
        {
            var path = Path.GetFullPath(Path.Combine(root, fileName));
            Directory.CreateDirectory(Directory.GetParent(path).FullName);

            var tpl = resources.First(r => r.Name == resourceName);
            var content = Template.Parse(tpl.Content).Render(data);

            File.WriteAllText(path, content);
        }
    }
}