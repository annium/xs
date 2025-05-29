using System.Collections.Generic;
using System.Xml.Linq;

namespace Annium.Xs.Cli.Dotnet.Extensions;

internal static class XElementExtensions
{
    public static XElement? GetElement(this XElement container, string name)
    {
        return container.Element(XName.Get(name, container.Name.NamespaceName));
    }

    public static IEnumerable<XElement> GetElements(this XElement container, string name)
    {
        return container.Elements(XName.Get(name, container.Name.NamespaceName));
    }
}
