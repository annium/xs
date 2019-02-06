using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Db.Dotnet
{
    internal class PackageRepository : IPackageRepository
    {
        private readonly IDotnetContext context;

        private readonly IMapper mapper;

        public PackageRepository(
            IDotnetContext context,
            IMapper mapper
        )
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task CreateAsync(Package package)
        {
            var entity = mapper.Map<Entities.Package>(package);

            context.Entry(entity).State = EntityState.Added;
            foreach (var dependency in entity.Dependencies)
                context.Entry(dependency).State = EntityState.Added;

            await context.SaveChangesAsync();
        }

        public async Task<Package> FindByNameVersionAsync(string name, string version)
        {
            var entity = await context.Packages
                .AsNoTracking()
                .Include(p => p.Dependencies)
                .Where(p => p.Name == name && p.Version == version)
                .FirstOrDefaultAsync();

            return mapper.Map<Package>(entity);
        }
    }
}