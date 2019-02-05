using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Xs.Registry.Core.Tools
{
    internal class RegistryConnector : IRegistryConnector
    {
        private readonly Uri sharedUri;

        private readonly ProjectType type;

        private readonly Uri uri;

        private CancellationTokenSource cts;

        public RegistryConnector(
            Uri sharedUri,
            ProjectType type,
            Uri uri
        )
        {
            this.sharedUri = sharedUri;
            this.type = type;
            this.uri = uri;
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
                            await httpClient.PostAsync(new Uri(sharedUri, $"/registry?type={type}&uri={uri}"), null);
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