using System.Runtime.Serialization;
using Annium.Core.Runtime.Types;
using Xx.Cli.Core.Models;

namespace Xx.Cli.Core.Tools;

public abstract record PlatformConfigurationBase
{
    [ResolutionKey]
    [DataMember(Order = 0)]
    public ProjectType Type { get; protected set; }
}
