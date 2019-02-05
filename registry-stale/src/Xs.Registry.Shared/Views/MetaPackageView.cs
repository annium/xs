// using System;
// using System.Collections.Generic;
// 

// namespace Xs.Registry.Shared.Models
// {
//     public class MetaPackageView
//     {
//         public string Name { get; }

//         public string Version { get; }

//         public string Description { get; }

//         public Instant Published { get; }

//         public uint Downloads { get; }

//         public string Owner { get; }

//         public IReadOnlyDictionary<PermissionCategory, Permission> Permissions { get; }

//         public PackagePreview(MetaPackage metaPackage)
//         {
//             Name = package.Name;
//             Version = package.Version;
//             Description = package.Description;
//             Published = package.Published;
//             Downloads = package.Downloads;
//             Owner = owner.Name;
//             Permissions = metaPackage.Permissions;
//         }
//     }
// }