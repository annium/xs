using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Tools
{
    internal class SpecialConfigurationManager : ISpecialConfigurationManager
    {
        public ProjectType Type { get; } = Constants.ProjectType;

        public void Save(string folder, IEnumerable<ValueTuple<string, Uri, string>> registries)
        {
            // TODO: implement
        }
    }
}