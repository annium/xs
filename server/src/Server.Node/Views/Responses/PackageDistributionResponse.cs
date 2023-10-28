namespace Server.Node.Views.Responses;

public sealed record PackageDistributionResponse(string Tarball, string Shasum, string Integrity);
