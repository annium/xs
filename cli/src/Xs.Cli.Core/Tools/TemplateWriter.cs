using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Annium.Extensions.Primitives;
using Annium.Logging.Abstractions;
using Scriban;
using Scriban.Runtime;
using Xs.Cli.Core.Helpers;

namespace Xs.Cli.Core.Tools
{
    internal class TemplateWriter : ITemplateWriter
    {
        private const string TemplateExtension = "tpl";
        private readonly ILogger<TemplateWriter> logger;
        private string root = Directory.GetCurrentDirectory();
        private string[] extensions = new string[]
        {
            "cs",
            "css",
            "d.ts",
            "dockerignore",
            "env",
            "gitignore",
            "html",
            "ico",
            "jest.config.js",
            "js",
            "json",
            "linguirc",
            "module.css",
            "module.scss",
            "scss",
            "ts",
            "tsx"
        };
        private IList<Resource> resources = new List<Resource>();

        public TemplateWriter(
            ILogger<TemplateWriter> logger
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
            if (resourceName.EndsWith(TemplateExtension))
            {
                logger.Trace($"Write template {resourceName} -> {path}");
                var scriptObject = new ScriptObject();
                scriptObject.Import(data);
                scriptObject.Import(typeof(StringExtensions));
                var ctx = new TemplateContext();
                ctx.PushGlobal(scriptObject);

                using var reader = new StreamReader(resource.Content);
                File.WriteAllText(path, Template.Parse(reader.ReadToEnd()).Render(ctx));
            }
            else
            {
                logger.Trace($"Write as is {resourceName} -> {path}");
                using var fs = File.Create(path);
                resource.Content.CopyTo(fs);
            }

            resources.Remove(resource);
        }

        public void WriteAll(object data)
        {
            foreach (var name in resources.ToArray().Select(r => r.Name))
                Write(name, toPath(stripExtension(name, TemplateExtension)), data);

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

            static string stripExtension(string name, string extension)
            {
                if (!endsWithExtension(name, extension))
                    return name;

                if (name.Length == extension.Length)
                    return string.Empty;

                return name.Substring(0, name.Length - extension.Length - 1);
            }

            static bool endsWithExtension(string name, string extension)
            {
                if (!name.EndsWith(extension))
                    return false;

                if (name.Length == extension.Length)
                    return true;

                var prevChar = name[name.Length - extension.Length - 1];

                return prevChar == '_' || prevChar == '.';
            }
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