namespace Annium.Xs.Cli.Node.Tools;

internal static class PackageName
{
    public static string GetPlainName(string name)
    {
        if (!name.StartsWith('@'))
            return name;

        return string.Join('-', name[1..].Split('/'));
    }
}
