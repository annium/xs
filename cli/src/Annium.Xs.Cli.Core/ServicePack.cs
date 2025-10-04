using System;
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
    public override void Configure(IServiceContainer container)
    {
        container.AddMapper();
        container.AddConfiguration<LoggerConfiguration>(builder => builder.AddCommandLineArgs());
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
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
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.For(BuildLogFilter(provider.Resolve<LoggerConfiguration>())).UseConsole());
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
