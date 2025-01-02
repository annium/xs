using System;
using System.Collections.Generic;
using Annium;
using Xx.Cli.Core.Tools;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Xx.Cli.Core.Models;

public sealed record SolutionConfiguration(
    string Directory,
    Uri Registry,
    string Token,
    IReadOnlyDictionary<ProjectType, Uri> Servers,
    PlatformConfigurationBase[] Types
)
{
    public static SolutionConfiguration Empty { get; } =
        new(
            string.Empty,
            new Uri("http://localhost"),
            string.Empty,
            new Dictionary<ProjectType, Uri>(),
            []
        );

    public Uri Registry { get; private set; } = Registry;
    public string Token { get; private set; } = Token;
    public IReadOnlyDictionary<ProjectType, Uri> Servers { get; private set; } = Servers;

    public void SetRegistry(Uri registry)
    {
        Registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public void SetToken(string token)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
    }

    public void SetServers(IReadOnlyDictionary<ProjectType, Uri> servers)
    {
        Servers = servers;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Directory, Registry, Token, HashCodeSeq.Combine(Servers), HashCodeSeq.Combine(Types));
    }
}
