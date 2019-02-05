namespace Xs.Registry.Db.Shared
{
    public class MetaPackagePermission
    {
        public PermissionCategory Category { get; }

        public Permission Permission { get; }

        public MetaPackagePermission(
            PermissionCategory category,
            Permission permission
        )
        {
            Category = category;
            Permission = permission;
        }
    }
}