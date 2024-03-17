using System;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Models;

public class ProjectTypeConfiguration
{
    public Uri Server { get; }
    public string Token { get; }
    public PlatformConfigurationBase? Special { get; }

    public ProjectTypeConfiguration(Uri server, string token, PlatformConfigurationBase? special)
    {
        Server = server;
        Token = token;
        Special = special;
    }
}
