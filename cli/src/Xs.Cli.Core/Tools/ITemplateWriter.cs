namespace Xs.Cli.Core.Tools
{
    public interface ITemplateWriter
    {
        void SetRoot(string root);

        void LoadResources(string prefix);

        void Write(string resourceName, string fileName, object data);

        void Copy(string resourceName, string fileName);
    }
}