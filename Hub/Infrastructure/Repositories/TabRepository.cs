using Hub.Application;
using Hub.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hub.Infrastructure.Repositories
{
    public class TabRepository : ITabRepository
    {
        private readonly HubDbContext _context;

        public TabRepository(HubDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Tab tab)
        {
            await _context.Tabs.AddAsync(tab);
        }

        public async Task<Tab> GetByUrlAndDateAsync(string domain, DateOnly date)
        {
            return await _context.Tabs
                .Include(t => t.Sessions)
                .FirstOrDefaultAsync(t => t.Url == domain && t.Date == date);
        }

        public async Task<Tab> GetByUrlAsync(string domain)
        {
            return await _context.Tabs
                .Include(t => t.Sessions)
                .FirstOrDefaultAsync(t => t.Url == domain);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
