using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Scriban;
using Scriban.Runtime;
using Xs.Cli.Core.Helpers;

namespace Xs.Cli.Core.Tools
{
    internal class TemplateWriter : ITemplateWriter, ILogSubject
    {
        public ILogger Logger { get; }
        private const string TemplateExtension = "tpl";
        private string _root = Directory.GetCurrentDirectory();

        private string[] _extensions =
        {
            "conf",
            "cs",
            "cshtml",
            "css",
            "d.ts",
            "dockerfile",
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
            "razor",
            "scss",
            "sh",
            "ts",
            "tsx"
        };

        private IList<Resource> _resources = new List<Resource>();

        public TemplateWriter(
            ILogger<TemplateWriter> logger
        )
        {
            Logger = logger;
        }

        public void LoadResources(string prefix)
        {
            _resources = ResourceLoader.Load(prefix, Assembly.GetCallingAssembly()).ToList();
        }

        public void SetRoot(string root)
        {
            Directory.CreateDirectory(root);
            _root = root;
        }

        public void AddExtensions(params string[] extensions)
        {
            foreach (var extension in extensions)
                if (string.IsNullOrWhiteSpace(extension) || extension.Trim() != extension)
                    throw new ArgumentException("Extension must be non - empty untrimmable string ");

            _extensions = _extensions.Concat(extensions).ToArray();
        }

        public void Write(string resourceName, string fileName, object data)
        {
            var path = Path.GetFullPath(Path.Combine(_root, fileName));
            var parent = Directory.GetParent(path) ?? throw new DirectoryNotFoundException($"Directory {path} has no parent");
            Directory.CreateDirectory(parent.FullName);

            var resource = _resources.First(r => r.Name == resourceName);
            if (resourceName.EndsWith(TemplateExtension))
            {
                this.Trace($"Write template {resourceName} -> {path}");
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
                this.Trace($"Write as is {resourceName} -> {path}");
                using var fs = File.Create(path);
                resource.Content.CopyTo(fs);
            }

            _resources.Remove(resource);
        }

        public void WriteAll(object data)
        {
            foreach (var name in _resources.ToArray().Select(r => r.Name))
                Write(name, ToPath(StripExtension(name, TemplateExtension)), data);

            string ToPath(string name)
            {
                var extension = _extensions
                    .Where(ext => EndsWithExtension(name, ext))
                    .OrderByDescending(ext => ext.Length)
                    .FirstOrDefault();

                if (extension == null)
                    return Path.Combine(name.Split('.'));

                return Path.Combine(StripExtension(name, extension).Split('.')) + $".{extension}";
            }

            static string StripExtension(string name, string extension)
            {
                if (!EndsWithExtension(name, extension))
                    return name;

                if (name.Length == extension.Length)
                    return string.Empty;

                return name.Substring(0, name.Length - extension.Length - 1);
            }

            static bool EndsWithExtension(string name, string extension)
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
            if (_resources.Count > 0)
                throw new InvalidOperationException(
                    $"{_resources.Count} not written:{Environment.NewLine}{string.Join(Environment.NewLine, _resources.Select(r => r.Name))}"
                );
        }
    }
}