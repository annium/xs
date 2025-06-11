using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Serialization.Abstractions;
using Constants = Annium.Serialization.Yaml.Constants;

namespace Annium.Xs.Cli.Commands.Sync;

public class SyncConfigurator
{
    private readonly string _configFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".xs.sync"
    );
    private readonly ISerializer<string> _serializer;

    public SyncConfigurator(IServiceProvider sp)
    {
        var serializerKey = SerializerKey.CreateDefault(Constants.MediaType);
        _serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
    }

    public List<SyncProject> Read()
    {
        if (!File.Exists(_configFile))
            return new();

        var raw = File.ReadAllText(_configFile);
        var projects = _serializer.Deserialize<List<SyncProject>>(raw);

        return projects;
    }

    public void Write(IReadOnlyCollection<SyncProject> projects)
    {
        var raw = _serializer.Serialize(projects.OrderBy(x => x.Path).ToArray());
        File.WriteAllText(_configFile, raw);
    }
}
