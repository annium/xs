namespace Xs.Registry.Abstract.Packages
{
    public class NoContentResult : IPackageResult
    {

    }

    public class NotFoundResult : IPackageResult
    {

    }

    public class ArrayResult<TPackage> : IPackageResult
    {
        public TPackage[] Packages { get; }

        public ArrayResult(TPackage[] packages)
        {
            Packages = packages;
        }
    }

    public class ForbiddenResult : IPackageResult
    {
        public string Error { get; }

        public ForbiddenResult(string error)
        {
            Error = error;
        }
    }

    public class ConflictResult : IPackageResult
    {
        public string Error { get; }

        public ConflictResult(string error)
        {
            Error = error;
        }
    }

    public class InternalErrorResult : IPackageResult
    {
        public string Error { get; }

        public InternalErrorResult(string error)
        {
            Error = error;
        }
    }

    public interface IPackageResult
    {

    }
}