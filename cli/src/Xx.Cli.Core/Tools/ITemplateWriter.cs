namespace Xx.Cli.Core.Tools;

public interface ITemplateWriter
{
    void SetRoot(string root);

    void AddExtensions(params string[] extensions);

    void LoadResources(string prefix);

    void Write(string resourceName, string fileName, object data);

    void WriteAll(object data);

    void EnsureAllWritten();
}
