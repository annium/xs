using System.Runtime.Serialization;
using Annium.Core.Reflection;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Tools
{
    public abstract class SpecialConfiguration
    {
        [ResolveField]
        [DataMember(Order = 0)]
        public ProjectType Type { get; protected set; }
    }
}