using System;

namespace Xs.Registry.Node.Client
{
    public class NodeClient
    {
        private Uri uri;

        internal void SetUri(Uri uri)
        {
            if (this.uri != null)
                throw new InvalidOperationException($"Uri already assigned");

            this.uri = uri;
        }
    }
}