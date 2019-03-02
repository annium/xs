namespace Xs.Cli.Core.Helpers
{
    public class Resource
    {
        public string Name { get; }

        public string Content { get; }

        public Resource(
            string name,
            string content
        )
        {
            Name = name;
            Content = content;
        }

        public void Deconstruct(
            out string name,
            out string content
        )
        {
            name = Name;
            content = Content;
        }
    }
}