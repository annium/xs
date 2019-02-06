using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Xs.Registry.Db.Shared
{
    internal class MetaPackageRepository : IMetaPackageRepository
    {
        private readonly ISharedContext context;

        private readonly IMapper mapper;

        public MetaPackageRepository(
            ISharedContext context,
            IMapper mapper
        )
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<MetaPackage> CreateAsync(MetaPackage metaPackage)
        {
            var entity = mapper.Map<Entities.MetaPackage>(metaPackage);

            context.Entry(entity).State = EntityState.Added;
            foreach (var permission in entity.Permissions)
                context.Entry(permission).State = EntityState.Added;

            await context.SaveChangesAsync();

            context.Entry(entity).State = EntityState.Detached;
            foreach (var permission in entity.Permissions)
                context.Entry(permission).State = EntityState.Detached;

            return mapper.Map<MetaPackage>(entity);
        }

        public async Task<MetaPackage> GetByIdAsync(Guid id)
        {
            var entity = await context.MetaPackages
                .AsNoTracking()
                .Include(p => p.Permissions)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            return mapper.Map<MetaPackage>(entity);
        }

        public async Task<MetaPackage> FindByTypeNameAsync(ProjectType type, string name)
        {
            var typeString = type.ToString();
            name = name.ToLower();

            var entity = await context.MetaPackages
                .AsNoTracking()
                .Include(p => p.Permissions)
                .Where(p => p.Type == typeString && p.LowerName == name)
                .FirstOrDefaultAsync();

            return mapper.Map<MetaPackage>(entity);
        }

        public async Task UpdateInfoAsync(Guid id, IPackageInfo packageInfo)
        {
            await context.MetaPackages
                .Where(p => p.Id == id)
                .UpdateAsync(u => new Entities.MetaPackage()
                {
                    Name = packageInfo.Name,
                        Version = packageInfo.Version,
                        Description = packageInfo.Description,
                        Published = packageInfo.Published,
                });
        }

        public async Task SetDownloadsAsync(Guid id, int downloads)
        {
            await context.MetaPackages
                .Where(p => p.Id == id)
                .UpdateAsync(u => new Entities.MetaPackage { Downloads = downloads });
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            await context.MetaPackages.Where(p => p.Id == id).DeleteAsync();
        }

        public async Task DeleteByTypeNameAsync(ProjectType type, string name)
        {
            var typeString = type.ToString();
            name = name.ToLower();

            await context.MetaPackages.Where(p => p.Type == typeString && p.LowerName == name).DeleteAsync();
        }
    }
}