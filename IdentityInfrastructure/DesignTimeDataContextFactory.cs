using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityInfrastructure
{
    /// <summary>
    /// This class allows the migrations to function by providing a temporary context will will be used
    /// when configuring the DbContext when Database.EnsureCreated() is called in the ItemMAnagementAPI BaseController
    /// </summary>
    public class DesignTimeDataContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
    {
        /// <summary>
        /// This method is called when EnsureCreated() is invoked to provide a data context for the Migration stuff to work
        /// Without this, the existence of multiple contexts confuses EF Migrations
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public IdentityDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<IdentityDbContext>();
            var connectionString = "Server=127.0.0.1,1402;Initial Catalog=Identity_DEV;Persist Security Info=False;User ID=sa;Password=KeepItSimple%1;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";
            builder.UseSqlServer(connectionString);

            return new IdentityDbContext(builder.Options);
        }
    }
}
