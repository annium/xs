using System;

namespace Xs.RegistryClient.Shared
{
    public abstract class ClientBase
    {
        private readonly ClientBase[] clients;

        protected Uri uri = new Uri("http://localhost");

        public ClientBase(params ClientBase[] clients)
        {
            this.clients = clients;
        }

        public void SetUri(Uri uri)
        {
            if (!this.uri.IsLoopback)
                throw new InvalidOperationException($"Uri already assigned.");

            foreach (var client in clients)
                client.SetUri(uri);
            this.uri = uri;
        }
    }
}