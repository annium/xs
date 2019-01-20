using Xs.Registry.Core.Client;

namespace Xs.Registry.Dotnet.Client
{
    public class DotnetClient : ClientBase, IProjectClient
    {
        public IInfoClient Info { get; }

        public DotnetClient(
            InfoClient infoClient
        ) : base(infoClient)
        {
            Info = infoClient;
        }
    }
}