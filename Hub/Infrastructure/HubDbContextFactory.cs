using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Hub.Infrastructure;

namespace Hub.Infrastructure
{
    public class HubDbContextFactory : IDesignTimeDbContextFactory<HubDbContext>
    {
        public HubDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HubDbContext>();
            optionsBuilder.UseSqlite("Data Source=hub.db");

            return new HubDbContext(optionsBuilder.Options);
        }
    }
}
