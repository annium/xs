using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;
using Xs.Cli.Core.Helpers;
using Xs.Cli.Core.Logging;

namespace Xs.Cli.Core.Tools
{
    internal class TemplateWriter : ITemplateWriter
    {
        private const string TplExtension = "tpl";

        private readonly ILogger logger;

        private string root = Directory.GetCurrentDirectory();

        private string[] extensions =
            new string[] { "cs", "css", "d.ts", "dockerignore", "env", "gitignore", "html", "ico", "js", "json", "scss", "ts", "tsx" };

        private IList<Resource> resources;

        public TemplateWriter(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public void LoadResources(string prefix)
        {
            resources = ResourceLoader.Load(prefix, Assembly.GetCallingAssembly()).ToList();
        }

        public void SetRoot(string root)
        {
            Directory.CreateDirectory(root);
            this.root = root;
        }

        public void AddExtensions(params string[] extensions)
        {
            foreach (var extension in extensions)
                if (string.IsNullOrWhiteSpace(extension) || extension.Trim() != extension)
                    throw new ArgumentException("Extension must be non - empty untrimmable string ");

            this.extensions = this.extensions.Concat(extensions).ToArray();
        }

        public void Write(string resourceName, string fileName, object data)
        {
            var path = Path.GetFullPath(Path.Combine(root, fileName));
            Directory.CreateDirectory(Directory.GetParent(path).FullName);

            var resource = resources.First(r => r.Name == resourceName);
            if (resourceName.EndsWith(TplExtension))
            {
                logger.Trace($"Write template {resourceName} -> {path}");
                File.WriteAllText(path, Template.Parse(new StreamReader(resource.Content).ReadToEnd()).Render(data));
            }
            else
            {
                logger.Trace($"Write as is {resourceName} -> {path}");
                using(var fs = File.Create(path)) resource.Content.CopyTo(fs);
            }

            resources.Remove(resource);
        }

        public void WriteAll(object data)
        {
            foreach (var name in resources.ToArray().Select(r => r.Name))
                Write(name, toPath(stripExtension(name, TplExtension)), data);

            string toPath(string name)
            {
                var extension = extensions
                    .Where(ext => endsWithExtension(name, ext))
                    .OrderByDescending(ext => ext.Length)
                    .FirstOrDefault();

                if (extension == null)
                    return Path.Combine(name.Split('.'));

                return Path.Combine(stripExtension(name, extension).Split('.')) + $".{extension}";
            }

            string stripExtension(string name, string extension) =>
                endsWithExtension(name, extension) ? name.Substring(0, name.Length - extension.Length - 1) : name;

            bool endsWithExtension(string name, string extension) => name.EndsWith(extension) &&
                (name[name.Length - extension.Length - 1] == '_' || name[name.Length - extension.Length - 1] == '.');
        }

        public void EnsureAllWritten()
        {
            if (resources.Count > 0)
                throw new InvalidOperationException(
                    $"{resources.Count} not written:{Environment.NewLine}{string.Join(Environment.NewLine, resources.Select(r => r.Name))}"
                );
        }
    }
}