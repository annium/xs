using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Xs.Registry.Db
{
    internal partial class Context : DbContext
    {
        public Context(DbContextOptions<Context> contextOptions) : base(contextOptions) { }

        public DataConnection GetDataConnection() => this.CreateLinqToDbConnection();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ConfigureDotnet(builder);
            ConfigureNode(builder);
            ConfigureShared(builder);
        }
    }
}