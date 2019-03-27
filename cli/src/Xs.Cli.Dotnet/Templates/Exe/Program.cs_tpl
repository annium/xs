using System;
using System.Threading;
using Annium.Extensions.Entrypoint;

namespace {{name}}
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            Console.WriteLine("Hello from {{name}}");
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}