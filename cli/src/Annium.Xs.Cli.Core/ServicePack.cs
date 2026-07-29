using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.CommandLine;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Data.Operations.Serialization.Json;
using Annium.Extensions.Shell;
using Annium.Logging;
using Annium.Logging.Console;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.NodaTime.Serialization.Json;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Serialization.Yaml;
using Annium.Xs.Cli.Core.Logging;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Core.Tasks.Dependencies;
using Annium.Xs.Cli.Core.Tools;
using YamlDotNet.Serialization.NamingConventions;

namespace Annium.Xs.Cli.Core;

public class ServicePack : ServicePackBase
{
    public override async Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.AddMapper();
        await container.AddConfigurationAsync<LoggerConfiguration>(
            x =>
            {
                x.AddCommandLineArgs();
            },
            ct
        );
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddTime().WithRealTime().SetDefault();

        container
            .AddSerializers()
            .WithJson(opts => opts.ConfigureForOperations().ConfigureForNodaTime(), isDefault: true)
            .WithYaml(
                (s, d) =>
                {
                    s.WithNamingConvention(CamelCaseNamingConvention.Instance);
                    d.WithNamingConvention(CamelCaseNamingConvention.Instance);
                    s.DisableAliases();
                }
            );
        container.AddHttpRequestFactory(true);
        container.AddLogging();
        container.AddShell();

        // projects
        container.Add<IProjectFactory, ProjectFactory>().Singleton();
        container.Add<IProjectLinker, ProjectLinker>().Singleton();

        RegisterTasks(container);

        // tools
        container.Add<IConfigurationManager, ConfigurationManager>().Singleton();
        container.Add<ITemplateWriter, TemplateWriter>().Transient();

        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.For(BuildLogFilter(provider.Resolve<LoggerConfiguration>())).UseConsole());

        return Task.CompletedTask;
    }

    private void RegisterTasks(IServiceContainer container)
    {
        // dependencies
        container.Add<AddPackageDependencyTask>().AsSelf().Singleton();
        container.Add<AddProjectDependencyTask>().AsSelf().Singleton();
        container.Add<DeletePackageDependencyTask>().AsSelf().Singleton();
        container.Add<DeleteProjectDependencyTask>().AsSelf().Singleton();

        // root
        container.Add<DiscoverChangedFilesTask>().AsSelf().Singleton();
        container.Add<DiscoverProjectsTask>().AsSelf().Singleton();
    }

    private Func<LogMessage<DefaultLogContext>, bool> BuildLogFilter(LoggerConfiguration cfg)
    {
        if (cfg.Trace)
            return _ => true;

        if (cfg.Debug)
            return m => m.Level >= LogLevel.Debug;

        return m => m.Level >= LogLevel.Info;
    }
}
