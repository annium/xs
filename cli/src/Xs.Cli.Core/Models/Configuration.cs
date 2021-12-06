using System;
using System.Collections.Generic;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Models;

public class Configuration
{
    public static Configuration Empty()
    {
        return new(
            string.Empty,
            new Uri("http://localhost"),
            string.Empty,
            new Dictionary<ProjectType, Uri>(),
            Array.Empty<SpecialConfiguration>()
        );
    }

    public string Directory { get; }
    public Uri Registry { get; private set; }
    public string Token { get; private set; }
    public IReadOnlyDictionary<ProjectType, Uri> Servers { get; private set; }
    public SpecialConfiguration[] Types { get; private set; }

    public Configuration(
        string directory,
        Uri registry,
        string token,
        IReadOnlyDictionary<ProjectType, Uri> servers,
        SpecialConfiguration[] types
    )
    {
        Directory = directory;
        Registry = registry;
        Token = token;
        Servers = servers;
        Types = types;
    }

    public void SetRegistry(Uri registry)
    {
        Registry = registry ??
                   throw new ArgumentNullException(nameof(registry));
    }

    public void SetToken(string token)
    {
        Token = token ??
                throw new ArgumentNullException(nameof(token));
    }

    public void SetServers(IReadOnlyDictionary<ProjectType, Uri> servers)
    {
        Servers = servers;
    }

    public void SetTypes(SpecialConfiguration[] types)
    {
        Types = types ??
                throw new ArgumentNullException(nameof(types));
    }
}