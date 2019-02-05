using System;

namespace Xs.Registry.Shared.Client
{
    public abstract class ClientBase
    {
        private readonly ClientBase[] clients;

        protected Uri uri;

        public ClientBase(params ClientBase[] clients)
        {
            this.clients = clients;
        }

        public void SetUri(Uri uri)
        {
            if (this.uri != null)
                throw new InvalidOperationException($"Uri already assigned.");

            foreach (var client in clients)
                client.SetUri(uri);
            this.uri = uri;
        }
    }
}