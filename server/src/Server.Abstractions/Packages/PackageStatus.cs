namespace Server.Abstractions.Packages;

public enum PackageStatus
{
    Ok,
    NotFound,
    Forbidden,
    Conflict,
    InternalError,
}