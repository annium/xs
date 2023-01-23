namespace Server.Node.Views.Responses;

public class PackageDistributionResponse
{
    public string Tarball { get; }

    public string Shasum { get; }

    public string Integrity { get; }

    public PackageDistributionResponse(
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