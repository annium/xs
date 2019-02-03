using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Core.Db
{
    internal class CoreDbContext : DbContext
    {
        public virtual DbSet<Models.User> Users { get; set; }

        public virtual DbSet<Models.UserSession> UserSessions { get; set; }

        public CoreDbContext(DbContextOptions<CoreDbContext> contextOptions) : base(contextOptions) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Models.UserSession>()
                .HasOne<Models.User>().WithMany().IsRequired()
                .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}