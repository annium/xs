using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Db
{
    internal partial class Context : DbContext
    {
        public Context(DbContextOptions<Context> contextOptions) : base(contextOptions) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ConfigureDotnet(builder);
            ConfigureNode(builder);
            ConfigureShared(builder);
        }
    }
}