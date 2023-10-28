using System;
using System.Collections.Generic;
using Annium;
using Xs.Cli.Core.Tools;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Xs.Cli.Core.Models;

public sealed record Configuration(
    string Directory,
    Uri Registry,
    string Token,
    IReadOnlyDictionary<ProjectType, Uri> Servers,
    SpecialConfiguration[] Types
)
{
    public static Configuration Empty { get; } =
        new(
            string.Empty,
            new Uri("http://localhost"),
            string.Empty,
            new Dictionary<ProjectType, Uri>(),
            Array.Empty<SpecialConfiguration>()
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
