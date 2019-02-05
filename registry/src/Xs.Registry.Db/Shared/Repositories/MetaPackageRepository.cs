using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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

        public async Task CreateAsync(MetaPackage metaPackage)
        {
            var entity = mapper.Map<Entities.MetaPackage>(metaPackage);

            context.Entry(entity).State = EntityState.Added;
            foreach (var permission in entity.Permissions)
                context.Entry(permission).State = EntityState.Added;

            await context.SaveChangesAsync();
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

        public async Task<MetaPackage> FindByProjectTypeNameAsync(ProjectType type, string name)
        {
            var typeString = type.ToString();

            var entity = await context.MetaPackages
                .AsNoTracking()
                .Include(p => p.Permissions)
                .Where(p => p.Type == typeString && p.Name == name)
                .FirstOrDefaultAsync();

            return mapper.Map<MetaPackage>(entity);
        }
    }
}