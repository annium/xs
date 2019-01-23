using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Xs.Cli.Core.Tools;
using Xs.Core.Models;

namespace Xs.Cli.Node.Tools
{
    internal class SpecialConfigurationManager : ISpecialConfigurationManager
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        private const string file = "nuget.config";

        private readonly string defaultName = "nuget";

        private readonly Uri defaultUri = new Uri("https://api.nuget.org/v3/index.json");

        public void Save(string folder, IEnumerable<ValueTuple<string, Uri, string>> registries)
        {
            var sources = new XElement(El.PackageSources);
            sources.Add(new XElement(El.Clear));

            foreach (var(name, uri, token) in registries)
                sources.Add(GetAddRule(name, uri));

            sources.Add(GetAddRule(defaultName, defaultUri));

            Save(folder, new XElement(El.Configuration, sources));
        }

        private void Save(string folder, XElement info)
        {
            var path = FilePath(folder);
            var xws = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = new string(' ', 2),
                OmitXmlDeclaration = false,
                Encoding = new UTF8Encoding(false),
            };

            using(var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using(var xw = XmlWriter.Create(fs, xws))
            {
                info.Save(xw);
            }
        }

        private XElement GetAddRule(string name, Uri uri) =>
            new XElement(El.Add, new XAttribute(El.Key, name), new XAttribute(El.Value, uri.ToString()));

        private string FilePath(string folder) => Path.Combine(folder, file);

        private static class El
        {
            public const string Configuration = "configuration";

            public const string PackageSources = "packageSources";

            public const string Clear = "clear";

            public const string Add = "add";

            public const string Key = "key";

            public const string Value = "value";
        }
    }
}