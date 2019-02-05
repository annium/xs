// using System;
// using System.IO;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using NodaTime;
// 
// using Xs.Execution;
// using Xs.Registry.Core.Auth;
// using Xs.Registry.Core.Db;
// using Xs.Registry.Core.Helpers;
// using Xs.Registry.Core.Models;
// using Xs.Registry.Core.Tools;
// using Xs.Registry.Dotnet.Helpers;
// using Xs.Registry.Dotnet.Models;
// using Xs.Registry.Dotnet.Payloads;
// using Xs.Registry.Dotnet.Storage;

// namespace Xs.Registry.Dotnet.Controllers
// {
//     public class PackagePublicationController : ServerController
//     {
//         private readonly Func<Instant> getInstant;

//         private readonly IMetaPackageManager metaPackageManager;

//         private readonly IMetaPackageRepository metaPackageRepository;

//         private readonly IPackageRepository<Package> packageRepository;

//         private readonly IPackageStorage packageStorage;

//         public PackagePublicationController(
//             Func<Instant> getInstant,
//             IMetaPackageManager metaPackageManager,
//             IMetaPackageRepository metaPackageRepository,
//             IPackageRepository<Package> packageRepository,
//             IPackageStorage packageStorage
//         )
//         {
//             this.getInstant = getInstant;
//             this.metaPackageManager = metaPackageManager;
//             this.metaPackageRepository = metaPackageRepository;
//             this.packageRepository = packageRepository;
//             this.packageStorage = packageStorage;
//         }

//         [HttpPut("api/v2/package")]
//         [Authorize(Access.Api)]
//         public async Task<IActionResult> PublishPackageAsync(CancellationToken ct)
//         {
//             using(var packageStream = await Request.GetUploadStreamOrNullAsync(ct))
//             {
//                 if (packageStream == null)
//                     return BadRequest("Use multipart/form-data to upload package.");

//                 var packagePayload = await readPackage(packageStream);

//                 var name = packagePayload.Name;
//                 var version = packagePayload.Version;

//                 // get metaPackage by (type, name)
//                 var metaPackage = await metaPackageRepository.FindByTypeNameAsync(Constants.ProjectType, name);

//                 // if missing - publish new package
//                 if (metaPackage == null)
//                     return await publishNewPackage(Executor.Staged(), GetUser(), packagePayload);

//                 // check version presence
//                 var isRepublished = await packageRepository.GetVersionExistsAsync(metaPackage.Id, version);

//                 // if present - republish package version, else - publish new package version
//                 return isRepublished ?
//                     await republishPackageVersion(Executor.Staged(), GetUser(), metaPackage, packagePayload) :
//                     await publishPackageVersion(Executor.Staged(), GetUser(), metaPackage, packagePayload);

//                 // // find existing and latest packages
//                 // var latest = await packageRepository.FindLatestByNameAsync(name);
//                 // var current = await packageRepository.FindByNameVersionAsync(name, version);

//                 // // try load metaPackage; if exists - check permissions
//                 // var metaPackage = latest == null ?
//                 //     metaPackageManager.Generate(user) :
//                 //     await metaPackageRepository.GetByIdAsync(latest.MetaPackageId);

//                 // // check publish permissions, if latest package found
//                 // if (latest != null && !metaPackageManager.CheckPermission(user, metaPackage, Permission.Publish))
//                 //     return Forbidden($"You need publish permission to publish package {name} {version}.");

//                 // // check republish permission if current package found - otherwise it's conflict
//                 // if (current != null && !metaPackageManager.CheckPermission(user, metaPackage, Permission.Republish))
//                 //     return Conflict($"Package {name} {version} already exists. You need republish permission to overwrite it.");

//                 // // if current package exists - delete it
//                 // if (current != null)
//                 //     executor.Stage(
//                 //         async() =>
//                 //         {
//                 //             await packageStorage.DeleteAsync(name, version);
//                 //             await packageRepository.DeleteByNameVersionAsync(name, version);
//                 //         },
//                 //         () => { }
//                 //     );

//                 // // persist to storage
//                 // executor.Stage(
//                 //     async() => await packageStorage.SaveAsync(
//                 //         name,
//                 //         version,
//                 //         packageStream,
//                 //         await packageReader.GetNuspecAsync(ct)
//                 //     ),
//                 //     () => packageStorage.DeleteAsync(name, version)
//                 // );

//                 // // persist to db
//                 // executor.Stage(
//                 //     () => packageRepository.SaveAsync(packagePayload),
//                 //     () => packageRepository.DeleteByNameVersionAsync(name, version)
//                 // );

//                 // // if no latest - save new metaPackage
//                 // if (latest == null)
//                 //     executor.Stage(
//                 //         () => metaPackageRepository.SaveAsync(metaPackage),
//                 //         () => { }
//                 //     );

//                 // await executor.RunAsync();
//             }

//             async Task<IActionResult> publishNewPackage(StageExecutor executor, User user, PackagePayload payload)
//             {
//                 var metaPackage = metaPackageManager.Generate(user, Constants.ProjectType, payload);

//                 executor.Stage(
//                     () => metaPackageRepository.CreateAsync(metaPackage),
//                     () => metaPackageRepository.DeleteByIdAsync(metaPackage.Id)
//                 );

//                 return await publishPackageVersion(executor, user, metaPackage, payload);
//             }

//             async Task<IActionResult> republishPackageVersion(StageExecutor executor, User user, MetaPackage metaPackage, PackagePayload payload)
//             {
//                 if (!metaPackageManager.CheckPermission(user, metaPackage, Permission.Unpublish))
//                     return Conflict($"Package {payload.Name} {payload.Version} already exists. You need republish permission to overwrite it.");

//                 executor.Stage(
//                     async() =>
//                     {
//                         await packageStorage.DeleteAsync(payload.Name, payload.Version);
//                         await packageRepository.DeleteByMetaPackageIdVersion(metaPackage.Id, payload.Version);
//                     },
//                     () => { }
//                 );

//                 return await publishPackageVersion(executor, user, metaPackage, payload);
//             }

//             async Task<IActionResult> publishPackageVersion(StageExecutor executor, User user, MetaPackage metaPackage, PackagePayload payload)
//             {
//                 if (!metaPackageManager.CheckPermission(user, metaPackage, Permission.Publish))
//                     return Forbidden($"You need publish permission to publish package {payload.Name} {payload.Version}.");

//                 var pkg = new Package(metaPackage.Id, metaPackage, payload.Name, payload.Version, payload.Description, payload.Published, 0, payload.Dependencies);

//                 executor.Stage(
//                     () => packageStorage.SaveAsync(pkg.Name, pkg.Version, payload.PackageStream, payload.NuspecStream),
//                     () => packageStorage.DeleteAsync(pkg.Name, pkg.Version)
//                 );

//                 executor.Stage(
//                     () => packageRepository.CreateAsync(pkg),
//                     () => packageRepository.DeleteByIdAsync(pkg.Id)
//                 );

//                 executor.Stage(
//                     () => metaPackageRepository.SyncDownloadsAsync(metaPackage.Id),
//                     () => metaPackageRepository.SyncDownloadsAsync(metaPackage.Id)
//                 );

//                 if (pkg.Version.CompareTo(metaPackage.Version) > 0)
//                     executor.Stage(
//                         () => { metaPackageManager.Update(metaPackage, pkg); return metaPackageRepository.UpdateAsync(metaPackage); },
//                         () => { }
//                     );

//                 await executor.RunAsync();

//                 return NoContent();
//             }

//             async Task<PackagePayload> readPackage(Stream packageStream)
//             {
//                 using(var packageReader = new NuGet.Packaging.PackageArchiveReader(packageStream, leaveStreamOpen : true))
//                 {
//                     await packageReader.ValidatePackageEntriesAsync(ct);

//                     var nuspec = packageReader.NuspecReader;
//                     var dependencies = nuspec.GetDependencyGroups()
//                         .SelectMany(g =>
//                         {
//                             var framework = g.TargetFramework.GetShortFolderName();
//                             return g.Packages.Select(d => new PackageDependency(framework, d.Id, d.VersionRange.ToNormalizedString()));
//                         })
//                         .ToArray();

//                     return new PackagePayload(
//                         nuspec.GetId(),
//                         nuspec.GetVersion().ToNormalizedString(),
//                         nuspec.GetDescription(),
//                         getInstant(),
//                         dependencies,
//                         packageStream,
//                         packageReader.GetNuspec()
//                     );
//                 }
//             }
//         }

//         [HttpDelete("api/v2/package/{name}/{version}")]
//         [Authorize(Access.Api)]
//         public async Task<IActionResult> UnpublishPackageAsync(string name, string version, CancellationToken token)
//         {
//             var allExisting = await packageRepository.FindAllByNameAsync(name);
//             var exists = allExisting.Any(e => e.Version == version);

//             if (!exists)
//                 return NotFound();

//             var user = GetUser();

//             // load metaPackage and check permissions
//             var metaPackageId = allExisting[0].MetaPackageId;
//             var metaPackage = await metaPackageRepository.GetByIdAsync(metaPackageId);
//             if (!metaPackageManager.CheckPermission(user, metaPackage, Permission.Unpublish))
//                 return Forbidden("You need unpublish permission to unpublish this package.");

//             var executor = Executor.Batch();

//             // delete from storage
//             executor.With(() => packageStorage.DeleteAsync(name, version));

//             // delete from db
//             executor.With(() => packageRepository.DeleteByNameVersionAsync(name, version));

//             // if it was last package - delete metaPackage
//             if (allExisting.Length == 1)
//                 executor.With(() => metaPackageRepository.DeleteByIdAsync(metaPackageId));

//             await executor.RunAsync();

//             return NoContent();
//         }
//     }
// }