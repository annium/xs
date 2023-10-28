namespace Server.Abstractions.Domain;

public enum PackageStatus
{
    Ok,
    NotFound,
    Forbidden,
    Conflict,
    InternalError,
}
