using System;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Models;

public class ProjectTypeConfiguration
{
    public Uri Server { get; }
    public string Token { get; }
    public PlatformConfigurationBase? Platform { get; }

    public ProjectTypeConfiguration(Uri server, string token, PlatformConfigurationBase? platform)
    {
        Server = server;
        Token = token;
        Platform = platform;
    }
}
