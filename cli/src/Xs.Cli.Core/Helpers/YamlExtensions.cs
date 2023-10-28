using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xs.Cli.Core.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

namespace Xs.Cli.Core.Helpers;

public static class Yaml
{
    public static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeInspector(inner => new DataContractTypeInspector(inner))
        .WithTypeConverter(new ProjectTypeTypeConverter())
        .WithTypeConverter(new UriTypeConverter())
        .Build();
}

internal class DataContractTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector _innerTypeInspector;

    public DataContractTypeInspector(ITypeInspector innerTypeInspector)
    {
        _innerTypeInspector = innerTypeInspector;
    }

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        var properties = _innerTypeInspector.GetProperties(type, container);

        return properties
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            .OrderBy(p => p.GetCustomAttribute<DataMemberAttribute>()?.Order ?? int.MaxValue)
            .ThenBy(x => x.Name);
    }
}

internal class UriTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Uri);

    public object ReadYaml(IParser parser, Type type) => throw new NotImplementedException();

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var @event = new Scalar(null, null, ((Uri)value!).ToString(), ScalarStyle.Any, true, false);
        emitter.Emit(@event);
    }
}

internal class ProjectTypeTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(ProjectType);

    public object ReadYaml(IParser parser, Type type) => throw new NotImplementedException();

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var @event = new Scalar(null, null, ((ProjectType)value!).ToString(), ScalarStyle.Any, true, false);
        emitter.Emit(@event);
    }
}
