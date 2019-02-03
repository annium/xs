// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Xs.Registry.Core.Models;
// using Xs.Registry.Core.Db;

// namespace Xs.Registry.Shared.Tools
// {
//     public class SearchManager<TPackage> : ISearchManager
//     where TPackage : IPackage
//     {
//         private readonly IPackageRepository<TPackage> packageRepository;

//         private readonly IMetaPackageRepository metaPackageRepository;

//         private readonly IUserRepository userRepository;

//         public SearchManager(
//             IPackageRepository<TPackage> packageRepository,
//             IMetaPackageRepository metaPackageRepository,
//             IUserRepository userRepository
//         )
//         {
//             this.packageRepository = packageRepository;
//             this.metaPackageRepository = metaPackageRepository;
//             this.userRepository = userRepository;
//         }

//         public async Task<PackagePreview[]> FindOwnerPackagesAsync(string ownerId, string query)
//         {
//             //TODO: implement distinct on BE
//             var packages = await packageRepository.FindAllByQueryAsync(query);
//             if (packages.Length == 0)
//                 return Array.Empty<PackagePreview>();

//             var user = await userRepository.GetByIdAsync(ownerId);
//             var metaPackage = await metaPackageRepository.FindAllByOwnerIdAsync(user.Id);

//             var result = new List<PackagePreview>();
//             foreach (var data in metaPackage)
//             {
//                 var package = packages.FirstOrDefault(p => p.MetaPackageId == data.Id);
//                 if (package != null)
//                     result.Add(new PackagePreview(package, data, user));
//             }

//             return result.ToArray();
//         }

//         public async Task<PackagePreview[]> FindPackagesAsync(string query, string ownerId = null)
//         {
//             //TODO: implement distinct on BE
//             var packages = await packageRepository.FindAllByQueryAsync(query);
//             if (packages.Length == 0)
//                 return Array.Empty<PackagePreview>();

//             var metaPackageIds = packages.Select(p => p.MetaPackageId).Distinct().ToArray();
//             var metaPackage = await metaPackageRepository.GetByIdsAsync(metaPackageIds);

//             var userIds = metaPackage.Select(p => p.OwnerId).ToArray();
//             var users = await userRepository.GetByIdsAsync(userIds);

//             var result = new List<PackagePreview>();
//             foreach (var package in packages)
//             {
//                 var data = metaPackage.FirstOrDefault(m => m.Id == package.MetaPackageId);
//                 if (data == null)
//                     throw new Exception($"MetaPackage {package.MetaPackageId} referenced by package {package.Id} is missing");
//                 var user = users.FirstOrDefault(u => u.Id == data.OwnerId);
//                 if (user == null)
//                     throw new Exception($"User {data.OwnerId} referenced by metaPackage {data.Id} is missing");

//                 result.Add(new PackagePreview(package, data, user));
//             }

//             return result.ToArray();
//         }

//         public async Task<PackagePreview> FindLatestPackageAsync(string name)
//         {
//             var package = await packageRepository.FindLatestByNameAsync(name);
//             if (package == null)
//                 return null;

//             return await BuildPackagePreviewAsync(package);
//         }

//         public async Task<PackagePreview> FindPackageAsync(string name, string version)
//         {
//             var package = await packageRepository.FindByNameVersionAsync(name, version);
//             if (package == null)
//                 return null;

//             return await BuildPackagePreviewAsync(package);
//         }

//         private async Task<PackagePreview> BuildPackagePreviewAsync(IPackage package)
//         {
//             var metaPackage = await metaPackageRepository.GetByIdAsync(package.MetaPackageId);
//             if (metaPackage == null)
//                 throw new Exception($"MetaPackage {package.MetaPackageId} referenced by package {package.Id} is missing");

//             var user = await userRepository.GetByIdAsync(metaPackage.OwnerId);
//             if (user == null)
//                 throw new Exception($"User {metaPackage.OwnerId} referenced by metaPackage {metaPackage.Id} is missing");

//             return new PackagePreview(package, metaPackage, user);
//         }
//     }
// }