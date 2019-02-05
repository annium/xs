using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Abstract.Tools
{
    internal class RegistryConnector
    {
        private readonly Uri mainUri;

        private readonly ProjectType type;

        private readonly Uri uri;

        private CancellationTokenSource cts;

        public RegistryConnector(
            ProjectType type,
            Uri uri,
            Uri mainUri
        )
        {
            this.type = type;
            this.uri = uri;
            this.mainUri = mainUri;
        }

        public void Connect()
        {
            cts = new CancellationTokenSource();
            Task.Run(async() =>
            {
                using(var httpClient = new HttpClient())
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await httpClient.PostAsync(new Uri(mainUri, $"/registry?type={type}&uri={uri}"), null);
                        }
                        catch { }
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                }
            });
        }

        public void Disconnect()
        {
            cts.Cancel();
        }
    }
}