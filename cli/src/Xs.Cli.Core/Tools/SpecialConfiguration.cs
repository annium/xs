using System.Runtime.Serialization;
using Annium.Core.Runtime.Types;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Tools
{
    public abstract class SpecialConfiguration
    {
        [ResolveField]
        [DataMember(Order = 0)]
        public ProjectType Type { get; protected set; } = null!;
    }
}