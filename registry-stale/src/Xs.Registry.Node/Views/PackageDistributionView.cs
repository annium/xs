namespace Xs.Registry.Node.Views
{
    public class PackageDistributionView
    {
        public string Tarball { get; }

        public string Shasum { get; }

        public string Integrity { get; }

        public PackageDistributionView(
            string tarball,
            string shasum,
            string integrity
        )
        {
            Tarball = tarball;
            Shasum = shasum;
            Integrity = integrity;
        }
    }
}